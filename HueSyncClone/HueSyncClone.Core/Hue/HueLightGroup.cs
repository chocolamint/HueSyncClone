using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HueSyncClone.Hue
{
    public class HueLightGroup
    {
        private readonly HueBridge _bridge;

        public string Id { get; set; }
        public string Name { get; set; }
        public IList<string> Lights { get; } = new List<string>();
        public string Type { get; set; }
        public LightState Action { get; set; }

        public HueLightGroup(HueBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task SetColorAsync(XyColor color, byte brightness, CancellationToken cancellationToken = default)
        {
            Action.On = true;
            Action.Xy = color;
            Action.Brightness = brightness;
            await _bridge.PutGroupStateAsync(this, new { on = true, xy = new[] { color.X, color.Y }, bri = brightness }, cancellationToken);
        }
    }
}