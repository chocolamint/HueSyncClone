using System;
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

        /// <summary>
        /// Gets a list of all scenes currently stored in the bridge.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Gets a list of all scenes currently stored in the bridge.
        /// Scenes are represented by a scene id, a name and a list of lights which are part of the scene.
        /// The name resource can contain a "friendly name" or can contain a unique code.
        /// Scenes are stored in the bridge.
        /// This means that scene light state settings can easily be retrieved by developers (using ADD link) and shown in their respective UI’s.
        /// Cached scenes (scenes stored with PUT) will be deprecated in the future.
        /// </para>
        /// <para>
        /// Additionally, bridge scenes should not be confused with the preset scenes stored in the Android and iOS Hue apps.
        /// In the apps these scenes are stored internally.
        /// Once activated they may then appear as a bridge scene.
        /// </para>
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a list of all scenes in the bridge.</returns>
        public async Task<IReadOnlyList<HueScene>> GetScenesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"http://{_ipAddress}/api/{UserName}/scenes", cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var root = JsonConvert.DeserializeObject<JObject>(json);

            var scenes = new List<HueScene>(root.Count);

            foreach (var p in root)
            {
                var scene = new HueScene { Id = p.Key };

                JsonConvert.PopulateObject(p.Value.ToString(), scene);

                scenes.Add(scene);
            }

            return scenes;
        }

        public Task SetSceneAsync(HueScene scene, CancellationToken cancellationToken = default) 
            => SetSceneAsync(scene.Id, cancellationToken);

        public async Task SetSceneAsync(string sceneId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsync(
                $"http://{_ipAddress}/api/{UserName}/groups/0/action",
                JsonContent(new { scene = sceneId }),
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        internal async Task PutLightStateAsync(HueLight light, object state, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var response = await _httpClient.PutAsync(
                $"http://{_ipAddress}/api/{UserName}/lights/{light.Id}/state",
                JsonContent(state),
                cancellationToken);

            Console.WriteLine($"PutAsync:{sw.ElapsedMilliseconds:N0}ms");

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
