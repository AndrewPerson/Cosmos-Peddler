using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace CosmosPeddler;

public partial class SolarSystem
{
    private static CacheDictionary<string, SolarSystem> systems = new();

    public static async Task<SolarSystem> GetSystem(string systemSymbol)
    {
        if (systems.TryGetValue(systemSymbol, out var cachedSystem))
        {
            return cachedSystem;
        }

        var system = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.GetSystemAsync(systemSymbol)
        )).Data;

        systems[systemSymbol] = system;

        return system;
    }

    // TODO Find a way to add this path to the OpenAPI Spec so it can be generated
    public static async Task<List<SolarSystem>> GetSystems()
    {
        var systems = await SpaceTradersClient.Retry429Policy.ExecuteAsync(async () => {
            using (var request = new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(SpaceTradersClient.Client.BaseUrl.TrimEnd('/') + "/systems.json"),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", SpaceTradersClient.Token)
                }
            })
            using (var response = await SpaceTradersClient.Client.HttpClient.SendAsync(request))
            {
                if (!response.IsSuccessStatusCode)
                {
                    //TODO Return proper values in this exception
                    throw new ApiException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, "", new Dictionary<string, IEnumerable<string>>(), null);
                }

                var completionSource = new TaskCompletionSource<List<SolarSystem>>();

                var json = await response.Content.ReadAsStringAsync();
                ThreadPool.QueueUserWorkItem(
                    data => data.completionSource.SetResult(JsonConvert.DeserializeObject<List<SolarSystem>>(data.json)),
                    (completionSource, json),
                    false
                );

                return await completionSource.Task;
            }
        });

        foreach (var system in systems)
        {
            SolarSystem.systems[system.Symbol] = system;
        }

        return systems;
    }

    public IAsyncEnumerable<Waypoint> GetWaypoints(int pageSize = 20) => Waypoint.GetSystemWaypoints(Symbol, pageSize);
}