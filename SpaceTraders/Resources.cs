using System;

public record Resources(string TradeSymbol, int Units);

public record Transaction(Resources Goods, int PricePerUnit, string ShipSymbol, string WaypointSymbol, DateTimeOffset Timestamp)
{
    public int TotalPrice => Goods.Units * PricePerUnit;
}