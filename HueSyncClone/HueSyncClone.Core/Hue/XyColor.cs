using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace HueSyncClone.Hue
{
    [DebuggerDisplay("({X}, {Y})")]
    [JsonConverter(typeof(XyColorConverter))]
    public struct XyColor : IEquatable<XyColor>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public double X { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public double Y { get; }

        public XyColor(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(XyColor other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is XyColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public void Deconstruct(out double x, out double y)
        {
            x = X;
            y = Y;
        }

        private class XyColorConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (XyColor)value;
                serializer.Serialize(writer, new[] { v.X, v.Y });
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var v = serializer.Deserialize<double[]>(reader);
                return new XyColor(v[0], v[1]);
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(XyColor);
        }
    }
}