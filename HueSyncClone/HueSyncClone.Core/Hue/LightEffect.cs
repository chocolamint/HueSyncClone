using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HueSyncClone.Hue
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LightEffect
    {
        None,
        ColorLoop
    }
}