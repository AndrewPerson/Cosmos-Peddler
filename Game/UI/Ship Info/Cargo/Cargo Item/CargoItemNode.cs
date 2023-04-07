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

		Market.GetMarket(nav.SystemSymbol, nav.WaypointSymbol).ContinueWith(t =>
		{
			var market = t.Result;

			if (market == null)
			{
                buy.Text = "Unavailable";
                buy.Disabled = true;
                sell.Text = "Unavailable";
                sell.Disabled = true;

                return;
			}

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
		if (marketItem == null)
		{
			PopupCreatorNode.CreatePopup(PopupType.Error, "Market item is null. Failed to open purchase UI.");
			return;
		}

		var nav = Data.Item2;

		buy.Text = "Loading...";
		buy.Disabled = true;

		Waypoint.GetWaypoint(nav.SystemSymbol, nav.WaypointSymbol).ContinueWith(t =>
		{
			buy.Text = $"Buy - ${marketItem.PurchasePrice}";
			buy.Disabled = false;

			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to fetch waypoint. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}

			PurchaseOrderNode.Show((marketItem, t.Result));
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	public void OpenSellUI()
	{
		if (marketItem == null)
		{
			PopupCreatorNode.CreatePopup(PopupType.Error, "Market item is null. Failed to open purchase UI.");
			return;
		}

		var nav = Data.Item2;

		sell.Text = "Loading...";
		sell.Disabled = true;

		Waypoint.GetWaypoint(nav.SystemSymbol, nav.WaypointSymbol).ContinueWith(t =>
		{
			sell.Text = $"Sell - ${marketItem.SellPrice}";
			sell.Disabled = false;

			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to fetch waypoint. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}

			SellOrderNode.Show((marketItem, t.Result));
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
