using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace HueSyncClone.Hue
{
    [JsonConverter(typeof(ColorTemparatureConverter))]
    [DebuggerDisplay("{Mired}M ({System.Math.Round(1000000.0 / Mired, 0)}K)")]
    public struct ColorTemperature : IEquatable<ColorTemperature>
    {
        public int Mired { get; }

        public ColorTemperature(int mired)
        {
            Mired = mired;
        }

        public bool Equals(ColorTemperature other)
        {
            return Mired == other.Mired;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ColorTemperature other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Mired;
        }

        public static explicit operator int(ColorTemperature colorTemperature) => colorTemperature.Mired;

        public static explicit operator ColorTemperature(int mired) => new ColorTemperature(mired);

        public class ColorTemparatureConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) 
                => serializer.Serialize(writer, ((ColorTemperature)value).Mired);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) 
                => new ColorTemperature(serializer.Deserialize<int>(reader));

            public override bool CanConvert(Type objectType) 
                => objectType == typeof(ColorTemperature);
        }
    }
}