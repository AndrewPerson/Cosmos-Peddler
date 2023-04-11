using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosPeddler;

public partial class Market
{
    private static CacheDictionary<string, CacheDictionary<string, Market?>> markets = new();

    public static async Task<Market?> GetMarket(string systemSymbol, string waypointSymbol)
    {
        if (markets.TryGetValue(systemSymbol, out var cachedSystemMarkets))
        {
            if (cachedSystemMarkets.TryGetValue(waypointSymbol, out var cachedMarket))
            {
                return cachedMarket;
            }
        }
        else markets.TryAdd(systemSymbol, new());

        var market = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.GetMarketAsync(systemSymbol, waypointSymbol)
        ).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                if (t.Exception != null)
                {
                    if (t.Exception.InnerException is ApiException e && e.StatusCode == 404)
                    {
                        return null;
                    }
                    else throw t.Exception;
                }
            }

            return t.Result;
        }))?.Data;

        markets[systemSymbol].TryAdd(waypointSymbol, market);

        return market;
    }

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

[Flags]
public enum MarketItemTradeType
{
    None = 0,
    Import = 1,
    Export = 2
}

public class MarketItem
{
    public MarketItemTradeType TradeType { get; set; }
    public TradeSymbol Symbol { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int TradeVolume { get; set; }
    public MarketTradeGoodSupply Supply { get; set; }
    public int PurchasePrice { get; set; }
    public int SellPrice { get; set; }
}