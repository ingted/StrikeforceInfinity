﻿using System.Drawing;

namespace Si.Library.ExtensionMethods
{
    public static class SiRectangleExtensions
    {
        /// <summary>
        /// Clones a float rectangle.
        /// </summary>
        public static RectangleF Clone(this RectangleF rectangle)
        {
            return new RectangleF(rectangle.Location, rectangle.Size);
        }

        public static RectangleF Balloon(this RectangleF rectangle, float x, float y)
        {
            var rec = rectangle.Clone();
            rec.Inflate(x, y);
            return rec;
        }

        public static RectangleF Balloon(this RectangleF rectangle, int x, int y)
        {
            var rec = rectangle.Clone();
            rec.Inflate(x, y);
            return rec;
        }

        public static RectangleF Balloon(this RectangleF rectangle, double x, double y)
        {
            var rec = rectangle.Clone();
            rec.Inflate((float)x, (float)y);
            return rec;
        }

        public static RectangleF Balloon(this RectangleF rectangle, float xy)
        {
            var rec = rectangle.Clone();
            rec.Inflate(xy, xy);
            return rec;
        }

        public static RectangleF Balloon(this RectangleF rectangle, int xy)
        {
            var rec = rectangle.Clone();
            rec.Inflate(xy, xy);
            return rec;
        }

        public static RectangleF Balloon(this RectangleF rectangle, double xy)
        {
            var rec = rectangle.Clone();
            rec.Inflate((float)xy, (float)xy);
            return rec;
        }

        /// <summary>
        /// Determines if the rectangle is inside of another rectangle.
        /// </summary>
        public static bool IntersectsWith(this RectangleF reference, RectangleF with, float tolerance)
        {
            return with.X < reference.X + reference.Width + tolerance
                && reference.X < with.X + with.Width + tolerance
                && with.Y < reference.Y + reference.Height + tolerance
                && reference.Y < with.Y + with.Height + tolerance;
        }
    }
}
