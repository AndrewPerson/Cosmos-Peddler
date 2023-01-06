using System;

namespace CosmosPeddler;

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
    public string Symbol { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int TradeVolume { get; set; }
    public MarketTradeGoodSupply Supply { get; set; }
    public int PurchasePrice { get; set; }
    public int SellPrice { get; set; }
}