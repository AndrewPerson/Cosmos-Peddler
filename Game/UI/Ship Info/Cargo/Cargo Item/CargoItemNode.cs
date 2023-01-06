using Godot;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class CargoItemNode : HBoxContainer
{
	private (ShipNav, ShipCargoItem) _cargoItemInfo = (null!, null!);

	public (ShipNav, ShipCargoItem) CargoItemInfo
	{
		get => _cargoItemInfo;
		set
		{
			_cargoItemInfo = value;
			UpdateCargoItem();
		}
	}

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

	private void UpdateCargoItem()
	{
		var (nav, cargo) = CargoItemInfo;

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

			var marketGood = goods.FirstOrDefault(i => i.Symbol == cargo.Symbol);

			if (marketGood == null)
			{
				buy.Text = "Unavailable";
				buy.Disabled = true;
				sell.Text = "Unavailable";
				sell.Disabled = true;

				return;
			}

			buy.Text = $"Buy {marketGood.PurchasePrice}";
			buy.Disabled = !marketGood.TradeType.HasFlag(MarketItemTradeType.Export);
			sell.Text = $"Sell {marketGood.SellPrice}";
			sell.Disabled = !marketGood.TradeType.HasFlag(MarketItemTradeType.Import);
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
