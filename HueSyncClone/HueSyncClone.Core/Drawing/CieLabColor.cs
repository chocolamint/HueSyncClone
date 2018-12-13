using System;

namespace HueSyncClone.Core.Drawing
{
    public struct CieLabColor : IEquatable<CieLabColor>
    {
        public double L { get; }
        public double A { get; }
        public double B { get; }

        public CieLabColor(double l, double a, double b)
        {
            L = l;
            A = a;
            B = b;
        }

        public static CieLabColor FromXyz(XyzColor color)
        {
            double F(double t) => t > 0.00885645167903563/* Math.Pow(6.0 / 29.0, 3) */
                ? Math.Pow(t, 1.0 / 3.0)
                : 7.78703703703704/* 1.0 / 3.0 * Math.Pow(29.0 / 6.0, 2)*/ * t + 0.137931034482759/* 4.0 / 29.0 */;

            var x = color.X * 100;
            var y = color.Y * 100;
            var z = color.Z * 100;

            var xn = 95.047;
            var yn = 100.0;
            var zn = 108.883;

            var fYPerYn = F(y / yn);

            return new CieLabColor
            (
                116.0 * fYPerYn - 16.0,
                500.0 * (F(x / xn) - fYPerYn),
                200.0 * (fYPerYn - F(z / zn))
            );
        }

        public bool Equals(CieLabColor other)
        {
            return L.Equals(other.L) && A.Equals(other.A) && B.Equals(other.B);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CieLabColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = L.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"({L}, {A}, {B})";
    }
}