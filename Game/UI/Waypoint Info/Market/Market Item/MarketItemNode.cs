using Godot;
using CosmosPeddler.Game.UI.PurchaseOrder;
using CosmosPeddler.Game.UI.SellOrder;

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

		name.Text = good.Symbol.ToString().ToHuman();
		name.TooltipText = good.Description;

		buy.Text = $"Buy - ${good.PurchasePrice}";
		buy.Disabled = !good.TradeType.HasFlag(MarketItemTradeType.Export);

		sell.Text = $"Sell - ${good.SellPrice}";
		sell.Disabled = !good.TradeType.HasFlag(MarketItemTradeType.Import);
	}

	public void OpenPurchaseUI()
	{
		PurchaseOrderNode.Show((Data.Item1, Data.Item2));
	}

	public void OpenSellUI()
	{
		SellOrderNode.Show((Data.Item1, Data.Item2));
	}
}
