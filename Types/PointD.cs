﻿using System;
using System.Drawing;

namespace AI2D.Types
{
    public class PointD
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointD()
        {
        }

        public static double DistanceTo(PointD from, PointD to)
        {
            var deltaX = Math.Pow((to.X - from.X), 2);
            var deltaY = Math.Pow((to.Y - from.Y), 2);
            return Math.Sqrt(deltaY + deltaX);
        }

        public static double AngleTo(PointD from, PointD to)
        {
            var fRadians = Math.Atan2((to.Y - from.Y), (to.X - from.X));
            var fDegrees = ((AngleD.RadiansToDegrees(fRadians) + 360.0) + AngleD.DegreeOffset) % 360.0;
            return fDegrees;
        }

        #region  Unary Operator Overloading.

        public static PointD operator -(PointD original, PointD modifier)
        {
            return new PointD(original.X - modifier.X, original.Y - modifier.Y);
        }

        public static PointD operator -(PointD original, double modifier)
        {
            return new PointD(original.X - modifier, original.Y - modifier);
        }

        public static PointD operator +(PointD original, PointD modifier)
        {
            return new PointD(original.X + modifier.X, original.Y + modifier.Y);
        }

        public static PointD operator +(PointD original, double modifier)
        {
            return new PointD(original.X + modifier, original.Y + modifier);
        }

        public static PointD operator *(PointD original, PointD modifier)
        {
            return new PointD(original.X * modifier.X, original.Y * modifier.Y);
        }

        public static PointD operator *(PointD original, double modifier)
        {
            return new PointD(original.X * modifier, original.Y * modifier);
        }

        public override bool Equals(object o)
        {
            return (Math.Round(((PointD)o).X, 4) == this.X && Math.Round(((PointD)o).Y, 4) == this.Y);
        }

        #endregion

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{{{Math.Round(X, 4).ToString("#.####")},{Math.Round(Y, 4).ToString("#.####")}}}";
        }

        public PointD(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public PointD(PointD p)
        {
            this.X = p.X;
            this.Y = p.Y;
        }

        public PointD(PointF p)
        {
            this.X = p.X;
            this.Y = p.Y;
        }

        public PointD(Point p)
        {
            this.X = p.X;
            this.Y = p.Y;
        }
    }
}
