using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CosmosPeddler;

public record RefineryConversion(Resources[] Consumed, Resources[] Produced);
public record WaypointChart(Waypoint Waypoint, string SubmittedBy, DateTimeOffset Timestamp);
public record WaypointSurvey(string Signature, string WaypointSymbol, string[] Deposits, DateTimeOffset Expiration, SurveySize Size);

public partial class Ship
{
    public static CacheDictionary<string, Cooldown> Cooldowns { get; } = new();
    private static CacheDictionary<string, Ship> ships = new();

    public static async Task<Ship> GetMyShip(string shipSymbol)
    {
        if (ships.TryGetValue(shipSymbol, out var cachedShip))
        {
            return cachedShip;
        }

        var ship = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.GetMyShipAsync(shipSymbol))
        ).Data;

        ships.TryAdd(shipSymbol, ship);

        return ship;
    }

    public static async IAsyncEnumerable<Ship> GetMyShips(int pageSize = 20)
    {
        if (ships.Complete)
        {
            foreach (var ship in ships.Values) yield return ship;
        }
        else
        {
            int itemsRemaining;
            int page = 1;
            do
            {
                var response = await SpaceTradersClient.Retry429Policy.ExecuteAsync(
                    () => SpaceTradersClient.Client.GetMyShipsAsync(page, pageSize)
                );

                itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

                foreach (var ship in response.Data)
                {
                    ships.TryAdd(ship.Symbol, ship);
                    yield return ship;
                }

                page++;
            }
            while (itemsRemaining > 0);

            ships.Complete = true;
        }
    }

    public static async Task<bool> WaypointHasShips(string systemSymbol, string waypointSymbol, int pageSize = 20)
    {
        if (!ships.Complete)
        {
            await foreach (var _ in GetMyShips(pageSize)) { };
        }

        return ships.Values.Any(ship =>
        {
            if (ship.Nav.Status == ShipNavStatus.IN_TRANSIT)
            {
                var route = ship.Nav.Route;

                if ((route.Departure.SystemSymbol == systemSymbol && route.Departure.Symbol == waypointSymbol) ||
                    (route.Destination.SystemSymbol == systemSymbol && route.Destination.Symbol == waypointSymbol))
                {
                    return true;
                }
            }
            else if (ship.Nav.SystemSymbol == systemSymbol && ship.Nav.WaypointSymbol == waypointSymbol)
            {
                return true;
            }

            return false;
        });
    }

    public static async Task<Ship> PurchaseShip(ShipType type, string shipyardWaypointSymbol)
    {
        var purchase = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.PurchaseShipAsync
        (
            new Body3
            {
                ShipType = type,
                WaypointSymbol = shipyardWaypointSymbol
            }
        ))).Data;

        Agent.MyAgent = purchase.Agent;
        ships.TryAdd(purchase.Ship.Symbol, purchase.Ship);

        return purchase.Ship;
    }

    public async Task Orbit()
    {
        var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.OrbitShipAsync(Symbol)
        )).Data;

        Nav = result.Nav;
    }

    public async Task Dock()
    {
        var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.DockShipAsync(Symbol)
        )).Data;

        Nav = result.Nav;
    }

    public async Task<Cooldown> GetCooldown()
    {
        if (Cooldowns.ContainsKey(Symbol))
        {
            return Cooldowns[Symbol];
        }
        else
        {
            var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
                () => SpaceTradersClient.Client.GetShipCooldownAsync(Symbol)
            )).Data;

            Cooldowns[Symbol] = result;
            return result;
        }
    }

    public async Task Refuel()
    {
        var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.RefuelShipAsync(Symbol)
        )).Data;

        Agent.MyAgent = result.Agent;
        Fuel = result.Fuel;
    }

    public async Task<RefineryConversion> Refine(Body4Produce produce)
    {
        var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.ShipRefineAsync
        (
            new Body4()
            {
                Produce = produce
            },
            Symbol
        ))).Data;

        Cargo = result.Cargo;

        Cooldowns[Symbol] = result.Cooldown;

        return new RefineryConversion(
            result.Consumed.Select(c => new Resources(c.TradeSymbol, c.Units)).ToArray(),
            result.Produced.Select(p => new Resources(p.TradeSymbol, p.Units)).ToArray()
        );
    }

    public async Task<Transaction> PurchaseCargo(string tradeSymbol, int units)
    {
        var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.PurchaseCargoAsync(new Body10()
        {
            Symbol = tradeSymbol,
            Units = units
        },
        Symbol))).Data;

        Agent.MyAgent = result.Agent;
        Cargo = result.Cargo;

        return new Transaction(
            new Resources(result.Transaction.TradeSymbol, result.Transaction.Units),
            result.Transaction.PricePerUnit,
            result.Transaction.ShipSymbol,
            result.Transaction.WaypointSymbol,
            result.Transaction.Timestamp
        );
    }

    public async Task<Transaction> SellCargo(string tradeSymbol, int units)
    {
        var result = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.SellCargoAsync(new SellCargoRequest()
        {
            Symbol = tradeSymbol,
            Units = units
        },
        Symbol))).Data;

        Agent.MyAgent = result.Agent;
        Cargo = result.Cargo;

        return new Transaction(
            new Resources(result.Transaction.TradeSymbol, result.Transaction.Units),
            result.Transaction.PricePerUnit,
            result.Transaction.ShipSymbol,
            result.Transaction.WaypointSymbol,
            result.Transaction.Timestamp
        );
    }

    public async Task JettisonCargo(string tradeSymbol, int units)
    {
        var jettisoned = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.JettisonAsync(new Body6()
        {
            Symbol = tradeSymbol,
            Units = units
        },
        Symbol))).Data;

        Cargo = jettisoned.Cargo;
    }

    public Task TransferCargo(string tradeSymbol, int units, string shipToSymbol)
    {
        throw new NotImplementedException();
    }

    public async Task NavigateTo(string destinationWaypointSymbol)
    {
        var nav = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.NavigateShipAsync
        (
            new Body8()
            {
                WaypointSymbol = destinationWaypointSymbol
            },
            Symbol
        ))).Data;

        Fuel = nav.Fuel;
        Nav = nav.Nav;
    }

    public async Task JumpTo(string destinationSystemSymbol)
    {
        var jump = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.JumpShipAsync
        (
            new Body7()
            {
                SystemSymbol = destinationSystemSymbol
            },
            Symbol
        ))).Data;

        Cooldowns[Symbol] = jump.Cooldown;
        Nav = jump.Nav;
    }

    public async Task WarpTo(string destinationWaypointSymbol)
    {
        var warp = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.WarpShipAsync
        (
            new Body9()
            {
                WaypointSymbol = destinationWaypointSymbol
            },
            Symbol
        ))).Data;

        Fuel = warp.Fuel;
        Nav = warp.Nav;
    }

    public async Task<ScannedSystem[]> ScanSystems()
    {
        var scan = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.CreateShipSystemScanAsync(Symbol)
        )).Data;

        Cooldowns[Symbol] = scan.Cooldown;

        return scan.Systems.ToArray();
    }

    public async Task<ScannedWaypoint[]> ScanWaypoints()
    {
        var scan = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.CreateShipWaypointScanAsync(Symbol)
        )).Data;

        Cooldowns[Symbol] = scan.Cooldown;

        return scan.Waypoints.ToArray();
    }

    public async Task<ScannedShip[]> ScanShips()
    {
        var scan = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.CreateShipShipScanAsync(Symbol)
        )).Data;

        Cooldowns[Symbol] = scan.Cooldown;

        return scan.Ships.ToArray();
    }

    public async Task<WaypointChart> ChartCurrentWaypoint()
    {
        var chart = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.CreateChartAsync(Symbol)
        )).Data;

        return new WaypointChart(
            chart.Waypoint,
            chart.Chart.SubmittedBy,
            chart.Chart.SubmittedOn
        );
    }

    public async Task<WaypointSurvey[]> SurveyCurrentWaypoint()
    {
        var survey = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.CreateSurveyAsync(Symbol)
        )).Data;

        Cooldowns[Symbol] = survey.Cooldown;

        return survey.Surveys.Select(s => new WaypointSurvey(
            s.Signature,
            s.Symbol,
            s.Deposits.Select(d => d.Symbol).ToArray(),
            s.Expiration,
            s.Size
        )).ToArray();
    }

    public async Task<Resources> ExtractResources(WaypointSurvey survey)
    {
        var resources = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(() => SpaceTradersClient.Client.ExtractResourcesAsync
        (
            new Body5
            {
                Survey = new Survey()
                {
                    Signature = survey.Signature,
                    Symbol = survey.WaypointSymbol,
                    Deposits = survey.Deposits.Select(d => new SurveyDeposit() { Symbol = d }).ToArray(),
                    Expiration = survey.Expiration,
                    Size = survey.Size
                }
            },
            Symbol
        ))).Data;

        Cargo = resources.Cargo;
        Cooldowns[Symbol] = resources.Cooldown;

        return new Resources(resources.Extraction.Yield.Symbol, resources.Extraction.Yield.Units);
    }
}