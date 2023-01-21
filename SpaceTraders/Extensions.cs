using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CosmosPeddler;

public static class StringExtensions
{
    public static string ToHuman(this string str)
    {
        return str.Split('_')
        .Where(s => s.Length > 0)
        .Select
        (
            s => s.Length == 1 ?
                    s.ToUpper() :
                    $"{char.ToUpper(s[0])}{s[1..].ToLower()}"
        )
        .Aggregate((a, b) => a + " " + b);
    }
}

public partial class SolarSystem
{
    public IAsyncEnumerable<Waypoint> GetWaypoints()
    {
        return SpaceTradersClient.GetSystemWaypoints(Symbol);
    }
}

public partial class Waypoint
{
    public Task<Shipyard?> GetShipyard()
    {
        return SpaceTradersClient.GetShipyard(SystemSymbol, Symbol).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    if (task.Exception.InnerException is ApiException e && e.StatusCode == 404)
                    {
                        return null;
                    }
                    else throw task.Exception;
                }
            }

            return task.Result;
        });
    }

    public Task<Market?> GetMarket()
    {
        return SpaceTradersClient.GetMarket(SystemSymbol, Symbol).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    if (task.Exception.InnerException is ApiException e && e.StatusCode == 404)
                    {
                        return null;
                    }
                    else throw task.Exception;
                }
            }

            return task.Result;
        });
    }

    public async IAsyncEnumerable<Ship> GetShips(int pageSize = 100)
    {
        await foreach (var ship in SpaceTradersClient.GetMyShips(pageSize))
        {
            if (ship.Nav.Status == ShipNavStatus.IN_TRANSIT)
            {
                if (ship.Nav.Route.Departure.Symbol == Symbol || ship.Nav.Route.Destination.Symbol == Symbol)
                {
                    yield return ship;
                }
            }
            else
            {
                if (ship.Nav.WaypointSymbol == Symbol)
                {
                    yield return ship;
                }
            }
        }
    }

    public async Task<bool> HasShips(int pageSize = 100)
    {
        var hasShip = false;
        await foreach (var _ in GetShips(pageSize))
        {
            hasShip = true;
            break;
        }

        return hasShip;
    }
}

public partial class Market
{
    public MarketItem[]? GetMarketItems()
    {
        if (TradeGoods == null) return null;

        var items = new MarketItem[TradeGoods.Count];

        var goods = TradeGoods.ToList();
        var imports = Imports.ToList();
        var exports = Exports.ToList();
        var exchanges = Exchange.ToList();

        for (int i = 0; i < items.Length; i++)
        {
            var import = imports.Find(g => g.Symbol.ToString() == goods[i].Symbol);
            var export = exports.Find(g => g.Symbol.ToString() == goods[i].Symbol);
            var exchange = exchanges.Find(g => g.Symbol.ToString() == goods[i].Symbol);

            var tradeType = MarketItemTradeType.None;
            if (import != null) tradeType |= MarketItemTradeType.Import;
            if (export != null) tradeType |= MarketItemTradeType.Export;
            if (exchange != null) tradeType |= MarketItemTradeType.Import | MarketItemTradeType.Export;

            items[i] = new MarketItem
            {
                TradeType = tradeType,
                Symbol = (import ?? export ?? exchange)!.Symbol,
                Name = (import ?? export ?? exchange)!.Name,
                Description = (import ?? export ?? exchange)!.Description,
                TradeVolume = goods[i].TradeVolume,
                Supply = goods[i].Supply,
                PurchasePrice = goods[i].PurchasePrice,
                SellPrice = goods[i].SellPrice
            };
        }

        return items.OrderByDescending(i => i.TradeType).ThenBy(i => i.Name).ToArray();
    }
}

public partial class Ship
{
    public Task<Data19> Refuel()
    {
        throw new System.Exception("Sike! This is a test! Really really really really really long text to simulate an error message.");
        return SpaceTradersClient.RefuelShip(Symbol);
    }

    public Task<Data20> PurchaseCargo(string tradeSymbol, int units)
    {
        return SpaceTradersClient.PurchaseCargo(tradeSymbol, units, Symbol);
    }

    public Task<Data16> SellCargo(string tradeSymbol, int units)
    {
        return SpaceTradersClient.SellCargo(tradeSymbol, units, Symbol);
    }
}