using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HueSyncClone.Hue
{
    /// <summary>
    /// Indicates hue light.
    /// </summary>
    [DebuggerDisplay("{Id,nq}: {Name,nq}")]
    public class HueLight
    {
        public const int MinHue = 0;
        public const int MaxHue = 65535;
        public const int MinSaturation = 0;
        public const int MaxSaturation = 254;
        public const int MinBrightness = 1;
        public const int MaxBrightness = 254;
        public const int MinColorTemperature = 153;
        public const int MaxColorTemperature = 500;

        private readonly HueBridge _bridge;

        public string Id { get; set; }

        /// <summary>
        /// Get or set details the state of the light.
        /// </summary>
        public LightState State { get; set; }

        /// <summary>
        /// Get or set a fixed name describing the type of light e.g. "Extended color light".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Get or set a unique, editable name given to the light.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or set the hardware model of the light.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Get or set the manufacturer name.
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// Get or set unique id of the device.
        /// The MAC address of the device with a unique endpoint id in the form: AA:BB:CC:DD:EE:FF:00:11-XX
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Get or set an identifier for the software version running on the light.
        /// </summary>
        [JsonProperty("swversion")]
        public string SoftwareVersion { get; set; }

        internal HueLight(HueBridge bridge)
        {
            _bridge = bridge;
        }

        public Task TurnOffAsync(CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = false }, cancellationToken);

        public Task SetColorAsync(HueSaturationColor color, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = true, hue = color.Hue, sat = color.Saturation }, cancellationToken);

        public Task SetColorAsync(XyColor color, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = true, xy = new[]{ color.X, color.Y } }, cancellationToken);

        public Task SetColorAsync(ColorTemperature color, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = true, ct = color.Mired }, cancellationToken);

        public Task SetBrightnessAsync(int brightness, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { bri = brightness }, cancellationToken);

        public Task SetColorAsync(HueSaturationColor color, int brightness, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = true, hue = color.Hue, sat = color.Saturation, bri = brightness }, cancellationToken);

        public Task SetColorAsync(XyColor color, int brightness, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = true, xy = new[] { color.X, color.Y }, bri = brightness }, cancellationToken);

        public Task SetColorAsync(ColorTemperature color, int brightness, CancellationToken cancellationToken = default)
            => _bridge.PutLightStateAndSynchronizeAsync(this, new { on = true, ct = color.Mired, bri = brightness }, cancellationToken);
    }
}