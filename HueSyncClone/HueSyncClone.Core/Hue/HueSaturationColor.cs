using System;

namespace HueSyncClone.Hue
{
    public struct HueSaturationColor: IEquatable<HueSaturationColor>
    {
        public int Hue { get; }
        public int Saturation { get; }

        public HueSaturationColor(int hue, int saturation)
        {
            Hue = hue;
            Saturation = saturation;
        }

        public bool Equals(HueSaturationColor other)
        {
            return Hue == other.Hue && Saturation == other.Saturation;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is HueSaturationColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Hue * 397) ^ Saturation;
            }
        }
    }
}