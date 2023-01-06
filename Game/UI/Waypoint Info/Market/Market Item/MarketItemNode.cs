using Godot;
using System;

namespace CosmosPeddler.Game;

public partial class MarketItemNode : HBoxContainer
{
	private MarketItem _item = null!;

	public MarketItem Item
	{
		get => _item;
		set
		{
			_item = value;
			UpdateItemInfo();
		}
	}

	private Label name = null!;
	private Button buy = null!;
	private Button sell = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		buy = GetNode<Button>("%Buy");
		sell = GetNode<Button>("%Sell");
	}

	private void UpdateItemInfo()
	{
		name.Text = Item.Symbol.Replace('_', ' ');
		name.TooltipText = Item.Description;

		buy.Text = $"Buy - ${Item.PurchasePrice}";
		buy.Disabled = !Item.TradeType.HasFlag(MarketItemTradeType.Export);

		sell.Text = $"Sell - ${Item.SellPrice}";
		sell.Disabled = !Item.TradeType.HasFlag(MarketItemTradeType.Import);
	}
}
