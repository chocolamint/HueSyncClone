using System;

namespace HueSyncClone.Drawing
{
    public struct CieLabColor : IEquatable<CieLabColor>
    {
        // 6.0 / 29.0
        private const double Delta = 0.206896551724138;
        private const double Xn = 95.047;
        private const double Yn = 100.0;
        private const double Zn = 108.883;

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
            double F(double t) => t > 0.00885645167903563/* Math.Pow(Delta, 3) */
                ? Math.Pow(t, 1.0 / 3.0)
                : 7.78703703703704/* 1.0 / (3.0 * Math.Pow(Delta, 2))*/ * t + 0.137931034482759/* 4.0 / 29.0 */;

            var x = color.X * 100;
            var y = color.Y * 100;
            var z = color.Z * 100;

            var fYPerYn = F(y / Yn);

            return new CieLabColor
            (
                Math.Round(116.0 * fYPerYn - 16.0, 4),
                Math.Round(500.0 * (F(x / Xn) - fYPerYn), 4),
                Math.Round(200.0 * (fYPerYn - F(z / Zn)), 4)
            );
        }

        public XyzColor ToXyzColor()
        {
            double NegateF(double t) => t > Delta
                ? Math.Pow(t, 3)
                : 3 * Math.Pow(Delta, 2) * (t - 4 / 29.0);

            var x = Xn * NegateF((L + 16) / 116 + A / 500);
            var y = Yn * NegateF((L + 16) / 116);
            var z = Zn * NegateF((L + 16) / 116 - B / 200);

            return new XyzColor
            (
                Math.Round(x / 100.0, 4),
                Math.Round(y / 100.0, 4),
                Math.Round(z / 100.0, 4)
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