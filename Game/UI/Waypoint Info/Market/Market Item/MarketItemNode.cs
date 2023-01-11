using Godot;
using CosmosPeddler.Game.UI.MarketOrder;

namespace CosmosPeddler.Game.UI.WaypointInfo;

public partial class MarketItemNode : ReactiveUI<(MarketItem, Waypoint)>
{
	private Label name = null!;
	private Button buy = null!;
	private Button sell = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		buy = GetNode<Button>("%Buy");
		sell = GetNode<Button>("%Sell");
	}

	public override void UpdateUI()
	{
		var good = Data.Item1;

		name.Text = good.Symbol.ToString().Replace('_', ' ');
		name.TooltipText = good.Description;

		buy.Text = $"Buy - ${good.PurchasePrice}";
		buy.Disabled = !good.TradeType.HasFlag(MarketItemTradeType.Export);

		sell.Text = $"Sell - ${good.SellPrice}";
		sell.Disabled = !good.TradeType.HasFlag(MarketItemTradeType.Import);
	}

	public void OpenPurchaseUI()
	{
		MarketOrderNode.Show((Data.Item1, MarketOrderType.Purchase, Data.Item2));
	}

	public void OpenSellUI()
	{
		MarketOrderNode.Show((Data.Item1, MarketOrderType.Sell, Data.Item2));
	}
}
