using System;
using System.Drawing;

namespace HueSyncClone.Core.Drawing
{
    public struct XyzColor : IEquatable<XyzColor>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public XyzColor(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static XyzColor FromRgb(Color color) 
        {
            double Linear(double c)
            {
                c = c / 255.0;
                return c > 0.04045 ? Math.Pow((c + 0.055) / 1.055, 2.4) : c / 12.92;
            }

            var r = Linear(color.R);
            var g = Linear(color.G);
            var b = Linear(color.B);

            return new XyzColor
            (
                0.4124 * r + 0.3576 * g + 0.1805 * b,
                0.2126 * r + 0.7152 * g + 0.0722 * b,
                0.0193 * r + 0.1192 * g + 0.9505 * b
            );
        }

        public bool Equals(XyzColor other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is XyzColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}