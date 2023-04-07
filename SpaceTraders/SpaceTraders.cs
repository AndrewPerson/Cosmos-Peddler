using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;

using Polly;
using Polly.Retry;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CosmosPeddler;

public partial class SpaceTradersClient
{
    public const string SAVE_FILE = "cosmos-peddler.json";

    public static string Token { get; private set; } = "";

    public static SpaceTradersClient Client { get; } = new(new HttpClient(new SpaceTradersHandler(new HttpClientHandler())));

    public static AsyncRetryPolicy Retry429Policy { get; } = Policy.Handle<ApiException>(e => e.StatusCode == 429)
                                                                    .WaitAndRetryForeverAsync
                                                                    (
                                                                        sleepDurationProvider: (retryCount, exception, _) =>
                                                                        {
                                                                            if (exception is ApiException apiException)
                                                                            {
                                                                                return TimeSpan.FromSeconds(apiException.Headers.TryGetValue("Retry-After", out var retryAfter) ? float.Parse(retryAfter.First()) : 2);
                                                                            }
                                                                            else return TimeSpan.FromSeconds(2);
                                                                        },
                                                                        onRetryAsync: (exception, _, time, _) =>
                                                                        {
                                                                            Godot.GD.Print($"429 received, retrying in {time.TotalSeconds} seconds");
                                                                            return Task.CompletedTask;
                                                                        }
                                                                    );

    public HttpClient HttpClient => _httpClient;

    partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
    {
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
    }

    /// <summary>
    /// Registers a new user and returns their token.
    /// </summary>
    /// <remarks>
    /// NOTE: This will not log the user in. Use the Login method for that.
    /// </remarks>
    public static async Task<string> Register(BodyFaction faction, string symbol)
    {
        var agentData = (await Retry429Policy.ExecuteAsync(() => Client.RegisterAsync
        (
            new Body
            {
                Faction = faction,
                Symbol = symbol
            }
        ))).Data;

        return agentData.Token;
    }

    // Not part of the Agent class because it's not part of the API and modifies internal state
    public static async Task<bool> Login(string token)
    {
        if (!await ValidToken(token)) return false;
        SpaceTradersClient.Token = token;

        return true;
    }

    public static async Task<bool> ValidToken(string token)
    {
        using var testClient = new HttpClient()
        {
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", token)
            }
        };

        try
        {
            var result = await Retry429Policy.ExecuteAsync(() => testClient.GetAsync(Client.BaseUrl.TrimEnd('/') + "/my/agent"));
            if (result != null && result.IsSuccessStatusCode) return true;
            else return false;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> Load(string dataDir)
    {
        if (!File.Exists(Path.Combine(dataDir, SAVE_FILE))) return false;

        using var stream = new FileStream(Path.Combine(dataDir, SAVE_FILE), FileMode.OpenOrCreate, FileAccess.Read);

        using var json = await JsonDocument.ParseAsync(stream);

        string token = json.RootElement.GetProperty("token").GetString()!;

        return await Login(token);
    }

    public static async Task Save(string dataDir)
    {
        await File.WriteAllTextAsync(Path.Combine(dataDir, SAVE_FILE), JsonSerializer.Serialize(new
        {
            Token
        }));
    }

    public static async IAsyncEnumerable<Faction> GetFactions(int pageSize = 20)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await Retry429Policy.ExecuteAsync(() => Client.GetFactionsAsync(page, pageSize));

            itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

            foreach (var faction in response.Data)
            {
                if (faction != null) yield return faction;
            }

            page++;
        }
        while (itemsRemaining > 0);
    }

    public static async Task<Faction> GetFaction(string factionSymbol)
    {
        return (await Retry429Policy.ExecuteAsync(() => Client.GetFactionAsync(factionSymbol))).Data;
    }

    public static async IAsyncEnumerable<Contract> GetContracts(int pageSize = 20)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await Retry429Policy.ExecuteAsync(() => Client.GetContractsAsync(page, pageSize));

            itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

            foreach (var contract in response.Data)
            {
                if (contract != null) yield return contract;
            }

            page++;
        }
        while (itemsRemaining > 0);
    }

    public static async Task<Contract> GetContract(string contractId)
    {
        return (await Retry429Policy.ExecuteAsync(() => Client.GetContractAsync(contractId))).Data;
    }

    public static async Task<Data2> AcceptContract(string contractId)
    {
        return (await Retry429Policy.ExecuteAsync(() => Client.AcceptContractAsync(contractId))).Data;
    }

    public static async Task<Data3> DeliverContract(string shipSymbol, string tradeSymbol, int units, string contractId)
    {
        return (await Retry429Policy.ExecuteAsync(() => Client.DeliverContractAsync
        (
            new Body2
            {
                ShipSymbol = shipSymbol,
                TradeSymbol = tradeSymbol,
                Units = units
            },
            contractId
        ))).Data;
    }

    public static async Task<Data4> FulfillContract(string contractId)
    {
        return (await Retry429Policy.ExecuteAsync(() => Client.FulfillContractAsync(contractId))).Data;
    }
}
