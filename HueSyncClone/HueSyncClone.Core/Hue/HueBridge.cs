using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HueSyncClone.Hue
{
    [DebuggerDisplay("Bridge({_ipAddress,nq})")]
    public class HueBridge
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _ipAddress;

        public string UserName { get; set; }

        public HueBridge(string ipAddress)
        {
            _ipAddress = ipAddress;
        }

        public static async Task<IReadOnlyList<HueBridge>> GetBridgesAsync()
        {
            var json = await _httpClient.GetStringAsync("https://www.meethue.com/api/nupnp");
            var result = JsonConvert.DeserializeObject<JObject[]>(json);
            return result.Select(x => new HueBridge(x["internalipaddress"].ToString())).ToList();
        }

        /// <summary>
        /// Authorize specified device type, and set the <see cref="UserName"/> property.
        /// </summary>
        /// <param name="deviceType">Device type you want to authorize.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>It is <see cref="Task"/> that will be completed when the button pressed on the bridge.</returns>
        public async Task AuthorizeAsync(string deviceType, CancellationToken cancellationToken)
        {
            while (true)
            {
                var response = await _httpClient.PostAsync($"http://{_ipAddress}/api", JsonContent(new { devicetype = deviceType }), cancellationToken);
                var body = await response.Content.ReadAsStringAsync();

                var results = (dynamic)JsonConvert.DeserializeObject(body);
                var result = results[0];

                if (result.success != null)
                {
                    UserName = result.success.username;
                    return;
                }

                await Task.Delay(1000, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Gets a list of all lights that have been discovered by the bridge.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns a list of all lights in the system.
        /// If there are no lights in the system then the bridge will return an empty.
        /// </returns>
        public async Task<IReadOnlyList<HueLight>> GetLightsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"http://{_ipAddress}/api/{UserName}/lights", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var root = JsonConvert.DeserializeObject<JObject>(json);

            var lights = new List<HueLight>(root.Count);

            foreach (var p in root)
            {
                var light = new HueLight(this) { Id = p.Key };

                JsonConvert.PopulateObject(p.Value.ToString(), light);

                lights.Add(light);
            }

            return lights;
        }

        public async Task<IReadOnlyList<HueLightGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"http://{_ipAddress}/api/{UserName}/groups", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var root = JsonConvert.DeserializeObject<JObject>(json);

            var groups = new List<HueLightGroup>(root.Count);

            foreach (var p in root)
            {
                var group = new HueLightGroup(this) { Id = p.Key };

                JsonConvert.PopulateObject(p.Value.ToString(), group);

                groups.Add(group);
            }

            return groups;
        }

        internal async Task PutLightStateAsync(HueLight light, object state, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PutAsync(
                $"http://{_ipAddress}/api/{UserName}/lights/{light.Id}/state",
                JsonContent(state),
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        internal async Task PutGroupStateAsync(HueLightGroup group, object state, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PutAsync(
                $"http://{_ipAddress}/api/{UserName}/groups/{group.Id}/action",
                JsonContent(state),
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        private static StringContent JsonContent(object content)
        {
            var json = JsonConvert.SerializeObject(content);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return stringContent;
        }
    }
}
