namespace CosmosPeddler;

public class MarketOrder
{
    public string ShipSymbol { get; set; } = "";
    public TradeSymbol Symbol { get; set; }
    public int Quantity { get; set; }
}