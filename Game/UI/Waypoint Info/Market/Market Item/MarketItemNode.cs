using Godot;

namespace CosmosPeddler.Game;

public partial class MarketItemNode : ReactiveUI<MarketItem>
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
		name.Text = Data.Symbol.Replace('_', ' ');
		name.TooltipText = Data.Description;

		buy.Text = $"Buy - ${Data.PurchasePrice}";
		buy.Disabled = !Data.TradeType.HasFlag(MarketItemTradeType.Export);

		sell.Text = $"Sell - ${Data.SellPrice}";
		sell.Disabled = !Data.TradeType.HasFlag(MarketItemTradeType.Import);
	}
}
