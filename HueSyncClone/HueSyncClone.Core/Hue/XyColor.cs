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

        public static XyColor FromRgb(int red, int green, int blue)
        {
            double ApplyGammaCorrection(double c) => c > 0.04045
                ? Math.Pow((c + 0.055) / 1.055, 2.4)
                : c / 12.92;

            var r = ApplyGammaCorrection(red);
            var g = ApplyGammaCorrection(green);
            var b = ApplyGammaCorrection(blue);

            var x = r * 0.664511 + g * 0.154324 + b * 0.162028;
            var y = r * 0.283881 + g * 0.668433 + b * 0.047685;
            var z = r * 0.000088 + g * 0.072310 + b * 0.986039;

            return Math.Abs(x + y + z) < double.Epsilon
                ? new XyColor(0, 0)
                : new XyColor(x / (x + y + z), y / (x + y + z));
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