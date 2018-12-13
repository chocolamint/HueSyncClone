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