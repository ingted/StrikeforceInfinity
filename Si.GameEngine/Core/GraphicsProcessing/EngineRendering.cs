﻿using NTDLS.Semaphore;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using Si.GameEngine.Utility;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Si.GameEngine.Core.GraphicsProcessing
{
    public class EngineRendering : IDisposable
    {
        public class CriticalRenderTargets
        {
            public BitmapRenderTarget IntermediateRenderTarget { get; set; }
            public WindowRenderTarget ScreenRenderTarget { get; set; }
        }


        public PessimisticCriticalResource<CriticalRenderTargets> RenderTargets { get; private set; } = new();
        public PrecreatedMaterials Materials { get; private set; }
        public PrecreatedTextFormats TextFormats { get; private set; }

        private readonly GameEngineCore _gameEngine;
        private readonly SharpDX.Direct2D1.Factory _direct2dFactory = new(FactoryType.SingleThreaded);
        private readonly SharpDX.DirectWrite.Factory _directWriteFactory = new();
        private readonly ImagingFactory _wicFactory = new();

        public EngineRendering(GameEngineCore gameEngine)
        {
            _gameEngine = gameEngine;

            var renderProp = new HwndRenderTargetProperties()
            {
                PresentOptions = PresentOptions.None,
                Hwnd = gameEngine.Display.DrawingSurface.Handle,
                PixelSize = new Size2(gameEngine.Display.NatrualScreenSize.Width, gameEngine.Display.NatrualScreenSize.Height)
            };

            var intermediateRenderTargetSize = new Size2F(_gameEngine.Display.TotalCanvasSize.Width, _gameEngine.Display.TotalCanvasSize.Height);

            //This is optional:
            var pixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);

            RenderTargets.Use(o =>
            {
                o.ScreenRenderTarget = new WindowRenderTarget(_direct2dFactory, new RenderTargetProperties(pixelFormat), renderProp)
                {
                    AntialiasMode = AntialiasMode.PerPrimitive //This is optional.
                };

                o.IntermediateRenderTarget = new BitmapRenderTarget(
                    o.ScreenRenderTarget,
                    CompatibleRenderTargetOptions.None,
                    intermediateRenderTargetSize)
                {
                    AntialiasMode = AntialiasMode.PerPrimitive //Optional.
                };

                Materials = new PrecreatedMaterials(o.ScreenRenderTarget);
                TextFormats = new PrecreatedTextFormats(_directWriteFactory);
            });

            var renderTargetProperties = new RenderTargetProperties(pixelFormat);
            var renderProperties = new HwndRenderTargetProperties
            {
                Hwnd = gameEngine.Display.DrawingSurface.Handle,
                PixelSize = new Size2(gameEngine.Display.NatrualScreenSize.Width, gameEngine.Display.NatrualScreenSize.Height),
                PresentOptions = PresentOptions.Immediately
            };
        }

        public void Dispose()
        {
            RenderTargets.Use(o =>
            {
                o.ScreenRenderTarget?.Dispose();
                o.ScreenRenderTarget?.Dispose();
            });
        }

        public string GetGraphicsAdaptersInfo()
        {
            var text = new StringBuilder();
            using (var factory = new SharpDX.DXGI.Factory1())
            {
                foreach (var adapter in factory.Adapters)
                {
                    string adapterName = adapter.Description.Description;
                    var videoMemory = adapter.Description.DedicatedVideoMemory / 1024.0 / 1024.0;

                    text.AppendLine($"\"{adapterName}\" : Dedicated Video Memory {videoMemory:n2}MB");
                }
            }

            return text.ToString();
        }

        public void ApplyScaling(CriticalRenderTargets renderTargets, float scale)
        {
            var sourceRect = CalculateCenterCopyRectangle(renderTargets.IntermediateRenderTarget.Size, scale);
            var destRect = new RawRectangleF(0, 0, _gameEngine.Display.NatrualScreenSize.Width, _gameEngine.Display.NatrualScreenSize.Height);
            renderTargets.ScreenRenderTarget.DrawBitmap(renderTargets.IntermediateRenderTarget.Bitmap, destRect, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear, sourceRect);
        }

        public static RawRectangleF CalculateCenterCopyRectangle(Size2F largerSize, float percentage)
        {
            if (percentage < -1 || percentage > 1)
            {
                throw new ArgumentException("Percentage must be in the range [-1, 1].");
            }

            float centerX = largerSize.Width * 0.5f;
            float centerY = largerSize.Height * 0.5f;

            float smallerWidth = largerSize.Width * percentage;
            float smallerHeight = largerSize.Height * percentage;

            float left = centerX - smallerWidth * 0.5f;
            float top = centerY - smallerHeight * 0.5f;
            float right = left + smallerWidth;
            float bottom = top + smallerHeight;

            if (percentage >= 0)
            {
                return new RawRectangleF(left, top, right, bottom);
            }
            else
            {
                return new RawRectangleF(right, bottom, left, top);
            }
        }

        public SharpDX.Direct2D1.Bitmap GetBitmap(Stream stream)
        {

            using (var decoder = new BitmapDecoder(_wicFactory, stream, DecodeOptions.CacheOnLoad))
            using (var frame = decoder.GetFrame(0))
            using (var converter = new FormatConverter(_wicFactory))
            {
                converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPBGRA);
                return RenderTargets.Use(o => SharpDX.Direct2D1.Bitmap.FromWicBitmap(o.ScreenRenderTarget, converter));
            }
        }

        /// <summary>
        /// Draws a bitmap at the specified location.
        /// </summary>
        /// <returns>Returns the rectangle that was calculated to hold the bitmap.</returns>
        public RawRectangleF DrawBitmapAt(RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap, double x, double y, double angle)
        {
            var destRect = new RawRectangleF((float)x, (float)y, (float)(x + bitmap.PixelSize.Width), (float)(y + bitmap.PixelSize.Height));
            SetTransformAngle(renderTarget, destRect, angle);
            renderTarget.DrawBitmap(bitmap, destRect, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            ResetTransform(renderTarget);
            return destRect;
        }

        /// Draws a bitmap from a specified location of a given size, to the the specified location.
        public RawRectangleF DrawBitmapAt(RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap,
            double x, double y, double angle, RawRectangleF sourceRect, Size2F destSize)
        {
            var destRect = new RawRectangleF((float)x, (float)y, (float)(x + destSize.Width), (float)(y + destSize.Height));
            SetTransformAngle(renderTarget, destRect, angle);
            renderTarget.DrawBitmap(bitmap, destRect, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear, sourceRect);
            ResetTransform(renderTarget);
            return destRect;
        }

        /// <summary>
        /// Draws a bitmap at the specified location.
        /// </summary>
        /// <returns>Returns the rectangle that was calculated to hold the bitmap.</returns>
        public RawRectangleF DrawBitmapAt(RenderTarget renderTarget, SharpDX.Direct2D1.Bitmap bitmap, double x, double y)
        {
            var destRect = new RawRectangleF((float)x, (float)y, (float)(x + bitmap.PixelSize.Width), (float)(y + bitmap.PixelSize.Height));
            renderTarget.DrawBitmap(bitmap, destRect, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            return destRect;
        }

        public RawRectangleF GetTextRext(double x, double y, string text, SharpDX.DirectWrite.TextFormat format)
        {
            using var textLayout = new SharpDX.DirectWrite.TextLayout(_directWriteFactory, text, format, float.MaxValue, float.MaxValue);
            return new RawRectangleF((float)x, (float)y, (float)(x + textLayout.Metrics.Width), (float)(y + textLayout.Metrics.Height));
        }

        public SizeF GetTextSize(string text, SharpDX.DirectWrite.TextFormat format)
        {
            //We have to check the size with some ending characters becuase TextLayout() seems to want to trim the text before calculating the metrics.
            using var textLayout = new SharpDX.DirectWrite.TextLayout(_directWriteFactory, $"[{text}]", format, float.MaxValue, float.MaxValue);
            using var spacerLayout = new SharpDX.DirectWrite.TextLayout(_directWriteFactory, "[]", format, float.MaxValue, float.MaxValue);
            return new SizeF(textLayout.Metrics.Width - spacerLayout.Metrics.Width, textLayout.Metrics.Height);
        }

        /// <summary>
        /// Draws text at the specified location.
        /// </summary>
        /// <returns>Returns the rectangle that was calculated to hold the text.</returns>
        public RawRectangleF DrawTextAt(RenderTarget renderTarget, double x, double y, double angle, string text, SharpDX.DirectWrite.TextFormat format, SolidColorBrush brush)
        {
            using var textLayout = new SharpDX.DirectWrite.TextLayout(_directWriteFactory, text, format, float.MaxValue, float.MaxValue);

            var textWidth = textLayout.Metrics.Width;
            var textHeight = textLayout.Metrics.Height;

            // Create a rectangle that fits the text
            var textRectangle = new RawRectangleF((float)x, (float)y, (float)(x + textWidth), (float)(y + textHeight));

            //DrawRectangleAt(renderTarget, textRectangle, 0, Materials.Raw.Blue, 0, 1);

            SetTransformAngle(renderTarget, textRectangle, angle);
            renderTarget.DrawText(text, format, textRectangle, brush);
            ResetTransform(renderTarget);

            return textRectangle;
        }

        public void DrawLine(RenderTarget renderTarget,
            double startPointX, double startPointY, double endPointX, double endPointY,
            SolidColorBrush brush, double strokeWidth = 1)
        {
            var startPoint = new RawVector2((float)startPointX, (float)startPointY);
            var endPoint = new RawVector2((float)endPointX, (float)endPointY);

            renderTarget.DrawLine(startPoint, endPoint, brush, (float)strokeWidth);
        }

        /// <summary>
        /// Draws a rectangle at the specified location.
        /// </summary>
        /// <returns>Returns the rectangle that was calculated to hold the Rectangle.</returns>
        public Ellipse FillEllipseAt(RenderTarget renderTarget, double x, double y, double radiusX, double radiusY, RawColor4 color)
        {
            var ellipse = new Ellipse()
            {
                Point = new RawVector2((float)x, (float)y),
                RadiusX = (float)radiusX,
                RadiusY = (float)radiusY,
            };

            renderTarget.FillEllipse(ellipse, new SolidColorBrush(renderTarget, color));

            return ellipse;
        }

        public void FillTriangleAt(RenderTarget renderTarget, double x, double y, double size, SolidColorBrush brush, double strokeWidth = 1)
        {
            // Define the points for the triangle
            RawVector2[] trianglePoints = new RawVector2[]
        {
                new RawVector2(0, (float)size),     // Vertex 1 (bottom-left)
                new RawVector2((float)size, (float)size),   // Vertex 2 (bottom-right)
                new RawVector2((float)(size / 2), 0)      // Vertex 3 (top-center)
        };

            // Create a PathGeometry and add the triangle to it
            var triangleGeometry = new PathGeometry(_direct2dFactory);
            using (GeometrySink sink = triangleGeometry.Open())
            {
                sink.BeginFigure(trianglePoints[0], FigureBegin.Filled);
                sink.AddLines(trianglePoints);
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
            }

            // Calculate the center of the triangle
            float centerX = (trianglePoints[0].X + trianglePoints[1].X + trianglePoints[2].X) / 3;
            float centerY = (trianglePoints[0].Y + trianglePoints[1].Y + trianglePoints[2].Y) / 3;

            // Calculate the adjustment needed to center the triangle at the desired position
            x -= centerX;
            y -= centerY;

            // Create a translation transform to move the triangle to the desired position
            renderTarget.Transform = new(
                1.0f, 0.0f,
                0.0f, 1.0f,
                (float)x, (float)y
            );

            renderTarget.DrawGeometry(triangleGeometry, brush, (float)strokeWidth);

            ResetTransform(renderTarget);
        }

        /// <summary>
        /// Draws a rectangle at the specified location.
        /// </summary>
        /// <returns>Returns the rectangle that was calculated to hold the Rectangle.</returns>
        public RawRectangleF DrawRectangleAt(RenderTarget renderTarget, RawRectangleF rect, double angle, RawColor4 color, double expand = 0, double strokeWidth = 1)
        {
            if (expand != 0)
            {
                rect.Left -= (float)expand;
                rect.Top -= (float)expand;
                rect.Bottom += (float)expand;
                rect.Right += (float)expand;
            }

            SetTransformAngle(renderTarget, rect, angle);
            renderTarget.DrawRectangle(rect, new SolidColorBrush(renderTarget, color), (float)strokeWidth);
            ResetTransform(renderTarget);

            return rect;
        }

        public void SetTransformAngle(RenderTarget renderTarget, RawRectangleF rect, double angle, RawMatrix3x2? existingMatrix = null)
        {
            angle = SiMath.DegreesToRadians(angle);

            float centerX = rect.Left + (rect.Right - rect.Left) / 2.0f;
            float centerY = rect.Top + (rect.Bottom - rect.Top) / 2.0f;

            // Calculate the rotation matrix
            float cosAngle = (float)Math.Cos(angle);
            float sinAngle = (float)Math.Sin(angle);

            var rotationMatrix = new RawMatrix3x2(
                cosAngle, sinAngle,
                -sinAngle, cosAngle,
                centerX - cosAngle * centerX + sinAngle * centerY,
                centerY - sinAngle * centerX - cosAngle * centerY
            );

            if (existingMatrix != null)
            {
                rotationMatrix = MultiplyMatrices(rotationMatrix, (RawMatrix3x2)existingMatrix);
            }

            renderTarget.Transform = rotationMatrix;
        }

        private RawMatrix3x2 GetScalingMatrix(double zoomFactor)
        {
            // Calculate the new center point (assuming your image dimensions are known)
            float centerX = _gameEngine.Display.TotalCanvasSize.Width / 2.0f;
            float centerY = _gameEngine.Display.TotalCanvasSize.Height / 2.0f;

            // Calculate the scaling transformation matrix
            var scalingMatrix = new RawMatrix3x2(
                (float)zoomFactor, 0,
                0, (float)zoomFactor,
                (float)(centerX * (1 - zoomFactor)),
                (float)(centerY * (1 - zoomFactor))
            );

            return scalingMatrix;
        }

        public static RawMatrix3x2 MultiplyMatrices(RawMatrix3x2 matrix1, RawMatrix3x2 matrix2)
        {
            return new RawMatrix3x2(
                matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21,
                matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22,
                matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21,
                matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22,
                matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix2.M31,
                matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix2.M32
            );
        }

        public void ResetTransform(RenderTarget renderTarget)
        {
            => renderTarget.Transform = new RawMatrix3x2(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        }
    }
}
