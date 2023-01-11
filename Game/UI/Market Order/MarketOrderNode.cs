using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosPeddler.Game.UI.ShipInfo;

namespace CosmosPeddler.Game.UI.MarketOrder;

public partial class MarketOrderNode : PopupUI<(MarketItem, MarketOrderType, Waypoint)>
{
	private bool loadedShips = false;
	private List<Ship> ships = null!;

	private Label name = null!;
	private OptionButton shipSelection = null!;
	private Button viewShip = null!;
	private SpinBox amount = null!;
	private Label price = null!;
	private Button complete = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		shipSelection = GetNode<OptionButton>("%Ships");
		viewShip = GetNode<Button>("%View Ship");
		amount = GetNode<SpinBox>("%Amount");
		price = GetNode<Label>("%Price");
		complete = GetNode<Button>("%Complete");
	}

	public override void UpdateUI()
	{
		if (Data.Item2 == MarketOrderType.Purchase) name.Text = $"Purchase {Data.Item1.Symbol.ToString().Replace('_', ' ')}";
		else name.Text = $"Sell {Data.Item1.Symbol.ToString().Replace('_', ' ')}";

		loadedShips = false;
		
		shipSelection.Clear();
		shipSelection.AddItem("Loading...");
		shipSelection.Disabled = true;
		shipSelection.Selected = 0;

		viewShip.Text = "Loading...";
		viewShip.Disabled = true;

		complete.Text = "Loading...";
		complete.Disabled = true;

		UpdateShips().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				//TODO Show error
				GD.PrintErr(t.Exception);
				return;
			}

			loadedShips = true;
			shipSelection.Clear();

			ships = t.Result;

			if (ships.Count > 0)
			{
				foreach (var ship in ships)
				{
					shipSelection.AddItem(ship.Symbol);
				}

				shipSelection.Disabled = false;
				shipSelection.Selected = 0;

				viewShip.Text = "View";
				viewShip.Disabled = false;

				if (Data.Item2 == MarketOrderType.Purchase)
				{
					complete.Text = "Purchase";
					complete.Disabled = false;
				}
				else
				{
					complete.Text = "Sell";
					complete.Disabled = false;
				}
			}
			else
			{
				shipSelection.AddItem("None");
				shipSelection.Selected = 0;

				viewShip.Text = "Unavailable";

				complete.Text = "Unavailable";
			}

			UpdateMaxItems();
		},
		TaskScheduler.FromCurrentSynchronizationContext());

		UpdateCost();
	}

	private async Task<List<Ship>> UpdateShips()
	{
		var ships = new List<Ship>();

		await foreach (var ship in Data.Item3.GetShips())
		{
			if (ship.Nav.Status == ShipNavStatus.DOCKED)
			{
				ships.Add(ship);
			}
		}

		return ships;
	}

	public void UpdateMaxItems()
	{
		if (ships.Count > 0)
		{
			var ship = ships[shipSelection.Selected];

			if (Data.Item2 == MarketOrderType.Purchase)
			{
				amount.MaxValue = ship.Cargo.Capacity - ship.Cargo.Units;
			}
			else
			{
				amount.MaxValue = ship.Cargo.Inventory.First(i => i.Symbol == Data.Item1.Symbol.ToString()).Units;
			}
		}
		else amount.MaxValue = 0;

		amount.Suffix = $"/ {amount.MaxValue}";
	}

	public void UpdateCost()
	{
		if (Data.Item2 == MarketOrderType.Purchase) price.Text = $"${amount.Value * Data.Item1.PurchasePrice}";
		else price.Text = $"${amount.Value * Data.Item1.SellPrice}";
	}

	public void OpenShipUI()
	{
		ShipInfoNode.Show(ships[shipSelection.Selected]);
	}

	public void Complete()
	{
		complete.Disabled = true;
		if (Data.Item2 == MarketOrderType.Purchase)
		{
			complete.Text = "Purchasing...";

			SpaceTradersClient.PurchaseCargo(
				Data.Item1.Symbol.ToString(),
				(int)amount.Value,
				ships[shipSelection.Selected].Symbol
			)
			.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					//TODO Show error
					GD.PrintErr(t.Exception);
					return;
				}

				complete.Text = "Purchase";
				complete.Disabled = false;

				Close();
			},
			TaskScheduler.FromCurrentSynchronizationContext());
		}
		else
		{
			complete.Text = "Selling...";

			SpaceTradersClient.SellCargo(
				Data.Item1.Symbol.ToString(),
				(int)amount.Value,
				ships[shipSelection.Selected].Symbol
			)
			.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					//TODO Show error
					GD.PrintErr(t.Exception);
					return;
				}

				complete.Text = "Sell";
				complete.Disabled = false;

				Close();
			},
			TaskScheduler.FromCurrentSynchronizationContext());
		}
	}

	public void Close()
	{
		UINode.Instance.HidePrevUI();
	}
}

public enum MarketOrderType
{
	Purchase,
	Sell
}
