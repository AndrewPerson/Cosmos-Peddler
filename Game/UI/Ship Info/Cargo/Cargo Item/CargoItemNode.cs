using Godot;
using System.Linq;
using System.Threading.Tasks;
using CosmosPeddler.Game.UI.PurchaseOrder;
using CosmosPeddler.Game.UI.SellOrder;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class CargoItemNode : ReactiveUI<(ShipCargoItem, ShipNav)>
{
	private MarketItem? marketItem;

	private Label units = null!;
	private Label name = null!;
	private Button buy = null!;
	private Button sell = null!;

	public override void _EnterTree()
	{
		units = GetNode<Label>("%Units");
		name = GetNode<Label>("%Name");
		buy = GetNode<Button>("%Buy");
		sell = GetNode<Button>("%Sell");
	}

	public override void UpdateUI()
	{
		var (cargo, nav) = Data;

		units.Text = $"{cargo.Units}x";
		name.Text = cargo.Name;

		buy.Text = "Loading...";
		buy.Disabled = true;
		sell.Text = "Loading...";
		sell.Disabled = true;

		SpaceTradersClient.GetMarket(nav.SystemSymbol, nav.WaypointSymbol).ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				if (t.Exception == null) return;

				if (t.Exception.InnerException is ApiException e)
				{
					if (e.StatusCode == 404)
					{
						buy.Text = "Unavailable";
						buy.Disabled = true;
						sell.Text = "Unavailable";
						sell.Disabled = true;

						return;
					}
				}

				throw t.Exception;
			}

			var market = t.Result;
			var goods = market.GetMarketItems();

			if (goods == null)
			{
				buy.Text = "Unavailable";
				buy.Disabled = true;
				sell.Text = "Unavailable";
				sell.Disabled = true;

				return;
			}

			marketItem = goods.FirstOrDefault(i => i.Symbol.ToString() == cargo.Symbol);

			if (marketItem == null)
			{
				buy.Text = "Unavailable";
				buy.Disabled = true;
				sell.Text = "Unavailable";
				sell.Disabled = true;

				return;
			}

			buy.Text = $"Buy - ${marketItem.PurchasePrice}";
			buy.Disabled = !marketItem.TradeType.HasFlag(MarketItemTradeType.Export);
			sell.Text = $"Sell - ${marketItem.SellPrice}";
			sell.Disabled = !marketItem.TradeType.HasFlag(MarketItemTradeType.Import);
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	public void OpenPurchaseUI()
	{
		//TODO Add null handling
		if (marketItem == null) return;

		var nav = Data.Item2;

		buy.Text = "Loading...";
		buy.Disabled = true;

		SpaceTradersClient.GetWaypoint(nav.SystemSymbol, nav.WaypointSymbol).ContinueWith(t =>
		{
			buy.Text = $"Buy - ${marketItem.PurchasePrice}";
			buy.Disabled = false;

			if (t.IsFaulted)
			{
				//TODO Show error
				GD.PrintErr(t.Exception);
				return;
			}

			PurchaseOrderNode.Show((marketItem, t.Result));
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	public void OpenSellUI()
	{
		//TODO Add null handling
		if (marketItem == null) return;

		var nav = Data.Item2;

		sell.Text = "Loading...";
		sell.Disabled = true;

		SpaceTradersClient.GetWaypoint(nav.SystemSymbol, nav.WaypointSymbol).ContinueWith(t =>
		{
			sell.Text = $"Sell - ${marketItem.SellPrice}";
			sell.Disabled = false;

			if (t.IsFaulted)
			{
				//TODO Show error
				GD.PrintErr(t.Exception);
				return;
			}

			SellOrderNode.Show((marketItem, t.Result));
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
