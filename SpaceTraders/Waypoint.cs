using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosPeddler;

public partial class Waypoint
{
    private static CacheDictionary<string, CacheDictionary<string, Waypoint>> waypoints = new();

    public static async Task<Waypoint> GetWaypoint(string systemSymbol, string waypointSymbol)
    {
        if (waypoints.TryGetValue(systemSymbol, out var cachedSystemWaypoints))
        {
            if (cachedSystemWaypoints.TryGetValue(waypointSymbol, out var cachedWaypoint))
            {
                return cachedWaypoint;
            }
        }
        else waypoints.TryAdd(systemSymbol, new());

        var waypoint = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.GetWaypointAsync(systemSymbol, waypointSymbol)
        )).Data;

        waypoints[systemSymbol].TryAdd(waypointSymbol, waypoint);

        return waypoint;
    }

    public static async IAsyncEnumerable<Waypoint> GetSystemWaypoints(string systemSymbol, int pageSize = 20)
    {
        if (waypoints.TryGetValue(systemSymbol, out var cachedSystemWaypoints))
        {
            if (cachedSystemWaypoints.Complete)
            {
                foreach (var waypoint in cachedSystemWaypoints.Values) yield return waypoint;
            }
        }
        else waypoints.TryAdd(systemSymbol, new());

        int itemsRemaining;
        int page = 1;
        do
        {
            var response = await SpaceTradersClient.Retry429Policy.ExecuteAsync(
                () => SpaceTradersClient.Client.GetSystemWaypointsAsync(page, pageSize, systemSymbol)
            );

            itemsRemaining = response.Meta.Total - response.Meta.Page * response.Meta.Limit;

            foreach (var waypoint in response.Data)
            {
                waypoints[systemSymbol].TryAdd(waypoint.Symbol, waypoint);
            }

            if (itemsRemaining <= 0) waypoints[systemSymbol].Complete = true;

            foreach (var waypoint in response.Data)
            {
                yield return waypoint;
            }

            page++;
        }
        while (itemsRemaining > 0);
    }

    public Task<Shipyard?> GetShipyard() => Shipyard.GetShipyard(SystemSymbol, Symbol);

    public Task<Market?> GetMarket() => Market.GetMarket(SystemSymbol, Symbol);

    public Task<JumpGate?> GetJumpGate() => JumpGate.GetJumpGate(SystemSymbol, Symbol);

    public async IAsyncEnumerable<Ship> GetShips(int pageSize = 20)
    {
        await foreach (var ship in Ship.GetMyShips(pageSize))
        {
            if (ship.Nav.Status == ShipNavStatus.IN_TRANSIT)
            {
                var route = ship.Nav.Route;

                if ((route.Departure.SystemSymbol == SystemSymbol && route.Departure.Symbol == Symbol) ||
                    (route.Destination.SystemSymbol == SystemSymbol && route.Destination.Symbol == Symbol))
                {
                    yield return ship;
                }
            }
            else
            {
                if (ship.Nav.SystemSymbol == SystemSymbol && ship.Nav.WaypointSymbol == Symbol)
                {
                    yield return ship;
                }
            }
        }
    }

    public async Task<bool> HasShips(int pageSize = 20) => await Ship.WaypointHasShips(SystemSymbol, Symbol, pageSize);
}
