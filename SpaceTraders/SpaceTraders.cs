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

namespace CosmosPeddler;

public partial class SpaceTradersClient
{
    public const string SAVE_FILE = "cosmos-peddler.json";

    private static string token = "";

    private static readonly SpaceTradersClient _client = new(new HttpClient());

    private static readonly AsyncRetryPolicy retry429Policy = Policy.Handle<ApiException>(e => e.StatusCode == 429)
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
                                                                        onRetryAsync: (_, _, time, _) =>
                                                                        {
                                                                            Godot.GD.Print($"429 received, retrying in {time.TotalSeconds} seconds");
                                                                            return Task.CompletedTask;
                                                                        }
                                                                    );

    partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
    {
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<bool> Login(string token)
    {
        if (!await ValidToken(token)) return false;
        SpaceTradersClient.token = token;

        return true;
    }

    public static async Task<bool> ValidToken(string token)
    {
        var existingToken = SpaceTradersClient.token;

        SpaceTradersClient.token = token;

        try
        {
            await retry429Policy.ExecuteAsync(_client.GetMyAgentAsync); 
            SpaceTradersClient.token = existingToken;
            return true;
        }
        catch
        {
            SpaceTradersClient.token = existingToken;
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
            token
        }));
    }

    /// <summary>
    /// Register a new user. NOTE: This will not log you in.
    /// </summary>
    public static async Task<Data> Register(BodyFaction faction, string symbol)
    {
        return (await retry429Policy.ExecuteAsync<Response>(() => _client.RegisterAsync
        (
            new Body
            {
                Faction = faction,
                Symbol = symbol
            }
        ))).Data;
    }

    public static async IAsyncEnumerable<SolarSystem> GetSystems(int pageSize = 100)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await retry429Policy.ExecuteAsync<Response2>(() => _client.GetSystemsAsync(page, pageSize));

            itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

            foreach (var system in response.Data)
            {
                if (system != null) yield return system;
            }

            page++;
        }
        while (itemsRemaining > 0);
    }

    public static async Task<SolarSystem> GetSystem(string systemSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response3>(() => _client.GetSystemAsync(systemSymbol))).Data;
    }

    public static async IAsyncEnumerable<Waypoint> GetSystemWaypoints(string systemSymbol, int pageSize = 100)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await retry429Policy.ExecuteAsync<Response4>(() => _client.GetSystemWaypointsAsync(page, pageSize, systemSymbol));

            itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

            foreach (var waypoint in response.Data)
            {
                if (waypoint != null) yield return waypoint;
            }

            page++;
        }
        while (itemsRemaining > 0);
    }

    public static async Task<Waypoint> GetWaypoint(string systemSymbol, string waypointSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response5>(() => _client.GetWaypointAsync(systemSymbol, waypointSymbol))).Data;
    }

    public static async Task<Market> GetMarket(string systemSymbol, string waypointSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response6>(() => _client.GetMarketAsync(systemSymbol, waypointSymbol))).Data;
    }

    public static async Task<Shipyard> GetShipyard(string systemSymbol, string waypointSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response7>(() => _client.GetShipyardAsync(systemSymbol, waypointSymbol))).Data;
    }

    public static async Task<JumpGate> GetJumpGate(string systemSymbol, string waypointSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response8>(() => _client.GetJumpGateAsync(systemSymbol, waypointSymbol))).Data;
    }

    public static async IAsyncEnumerable<Faction> GetFactions(int pageSize = 100)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await retry429Policy.ExecuteAsync<Response9>(() => _client.GetFactionsAsync(page, pageSize));

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
        return (await retry429Policy.ExecuteAsync<Response10>(() => _client.GetFactionAsync(factionSymbol))).Data;
    }

    public static async Task<Agent> GetMyAgent()
    {
        return (await retry429Policy.ExecuteAsync<Response11>(_client.GetMyAgentAsync)).Data;
    }

    public static async IAsyncEnumerable<Contract> GetContracts(int pageSize = 100)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await retry429Policy.ExecuteAsync<Response12>(() => _client.GetContractsAsync(page, pageSize));

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
        return (await retry429Policy.ExecuteAsync<Response13>(() => _client.GetContractAsync(contractId))).Data;
    }

    public static async Task<Data2> AcceptContract(string contractId)
    {
        return (await retry429Policy.ExecuteAsync<Response14>(() => _client.AcceptContractAsync(contractId))).Data;
    }

    public static async Task<Data3> DeliverContract(string shipSymbol, string tradeSymbol, int units, string contractId)
    {
        return (await retry429Policy.ExecuteAsync<Response15>(() => _client.DeliverContractAsync
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
        return (await retry429Policy.ExecuteAsync<Response16>(() => _client.FulfillContractAsync(contractId))).Data;
    }

    public static async IAsyncEnumerable<Ship> GetMyShips(int pageSize = 100)
    {
        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await retry429Policy.ExecuteAsync<Response17>(() => _client.GetMyShipsAsync(page, pageSize));

            itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

            foreach (var ship in response.Data)
            {
                if (ship != null) yield return ship;
            }

            page++;
        }
        while (itemsRemaining > 0);
    }

    public static async Task<Data5> PurchaseShip(ShipType shipType, string waypointSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response18>(() => _client.PurchaseShipAsync
        (
            new Body3
            {
                ShipType = shipType,
                WaypointSymbol = waypointSymbol
            }
        ))).Data;
    }

    public static async Task<Ship> GetMyShip(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response19>(() => _client.GetMyShipAsync(shipSymbol))).Data;
    }

    public static async Task<ShipNav> OrbitShip(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response20>(() => _client.OrbitShipAsync(shipSymbol))).Data.Nav;
    }

    public static async Task<Data7> ShipRefine(Body4Produce produce, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response21>(() => _client.ShipRefineAsync
        (
            new Body4()
            {
                Produce = produce
            },
            shipSymbol
        ))).Data;
    }

    public static async Task<Data8> CreateChart(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response22>(() => _client.CreateChartAsync(shipSymbol))).Data;
    }

    public static async Task<Cooldown> GetShipCooldown(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response23>(() => _client.GetShipCooldownAsync(shipSymbol))).Data;
    }

    public static async Task<ShipNav> DockShip(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response24>(() => _client.DockShipAsync(shipSymbol))).Data.Nav;
    }

    public static async Task<Data10> CreateSurvey(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response25>(() => _client.CreateSurveyAsync(shipSymbol))).Data;
    }

    public static async Task<Data11> ExtractResources(Survey survey, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response26>(() => _client.ExtractResourcesAsync
        (
            new Body5
            {
                Survey = survey
            },
            shipSymbol
        ))).Data;
    }

    public static async Task<ShipCargo> Jettison(string tradeSymbol, int units, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response27>(() => _client.JettisonAsync
        (
            new Body6
            {
                Symbol = tradeSymbol,
                Units = units
            },
            shipSymbol
        ))).Data.Cargo;
    }

    public static async Task<Data13> JumpShip(string systemSymbol, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response28>(() => _client.JumpShipAsync
        (
            new Body7
            {
                SystemSymbol = systemSymbol
            },
            shipSymbol
        ))).Data;
    }

    public static async Task<Data14> NavigateShip(string waypointSymbol, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response29>(() => _client.NavigateShipAsync
        (
            new Body8
            {
                WaypointSymbol = waypointSymbol
            },
            shipSymbol
        ))).Data;
    }

    public static async Task<Data15> WarpShip(string destinationWaypointSymbol, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response30>(() => _client.WarpShipAsync
        (
            new Body9
            {
                WaypointSymbol = destinationWaypointSymbol
            },
            shipSymbol
        ))).Data;
    }

    public static async Task<Data16> SellCargo(SellCargoRequest request, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response31>(() => _client.SellCargoAsync(request, shipSymbol))).Data;
    }

    public static async Task<Data17> CreateShipSystemScan(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response32>(() => _client.CreateShipSystemScanAsync(shipSymbol))).Data;
    }

    public static async Task<Data18> CreateShipShipScan(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response33>(() => _client.CreateShipShipScanAsync(shipSymbol))).Data;
    }

    public static async Task<Data19> RefuelShip(string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response34>(() => _client.RefuelShipAsync(shipSymbol))).Data;
    }

    public static async Task<Data20> PurchaseCargo(string tradeSymbol, int units, string shipSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response35>(() => _client.PurchaseCargoAsync
        (
            new Body10
            {
                Symbol = tradeSymbol,
                Units = units
            },
            shipSymbol
        ))).Data;
    }

    public static async Task<ShipCargo> TransferCargo(string tradeSymbol, int units, string shipFromSymbol, string shipToSymbol)
    {
        return (await retry429Policy.ExecuteAsync<Response36>(() => _client.TransferCargoAsync
        (
            new Body11
            {
                TradeSymbol = tradeSymbol,
                Units = units,
                ShipSymbol = shipToSymbol
            },
            shipFromSymbol
        ))).Data.Cargo;
    }
}