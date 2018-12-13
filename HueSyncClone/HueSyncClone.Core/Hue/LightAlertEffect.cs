using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueSyncClone.Hue
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LightAlertEffect
    {
        /// <summary>
        /// The light is not performing an alert effect.
        /// </summary>
        None,

        /// <summary>
        /// The light is performing one breathe cycle.
        /// </summary>
        Select,

        /// <summary>
        /// The light is performing breathe cycles for 15 seconds or until an <see cref="None"/> command is received.
        /// </summary>
        LSelect
    }
}