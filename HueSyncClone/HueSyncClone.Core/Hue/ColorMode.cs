using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueSyncClone.Hue
{
    /// <summary>
    /// Indicates the color mode in which the light is working, this is the last command type it received.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ColorMode
    {
        /// <summary>
        /// Hue and Saturation.
        /// </summary>
        [EnumMember(Value = "hs")]
        HueSaturation,

        /// <summary>
        /// XY.
        /// </summary>
        Xy,

        /// <summary>
        /// Color Temperature.
        /// </summary>
        [EnumMember(Value = "ct")]
        ColorTemperature
    }
}