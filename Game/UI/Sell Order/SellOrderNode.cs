using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosPeddler.Game.UI.ShipInfo;

namespace CosmosPeddler.Game.UI.SellOrder;

public partial class SellOrderNode : PopupUI<(MarketItem, Waypoint), SellOrderNode>
{
	private List<Ship> ships = null!;

	private Label name = null!;
	private OptionButton shipSelection = null!;
	private Button viewShip = null!;
	private SpinBox amount = null!;
	private Label price = null!;
	private Button sell = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		shipSelection = GetNode<OptionButton>("%Ships");
		viewShip = GetNode<Button>("%View Ship");
		amount = GetNode<SpinBox>("%Amount");
		price = GetNode<Label>("%Price");
		sell = GetNode<Button>("%Sell");
	}

	public override void UpdateUI()
	{
		name.Text = $"Sell {Data.Item1.Symbol.ToString().ToHuman()}";
		
		shipSelection.Clear();
		shipSelection.AddItem("Loading...");
		shipSelection.Disabled = true;
		shipSelection.Selected = 0;

		viewShip.Text = "Loading...";
		viewShip.Disabled = true;

		sell.Text = "Loading...";
		sell.Disabled = true;

		UpdateShips().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to sell. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}

            viewShip.Text = "View";
            sell.Text = "Sell";

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

				viewShip.Disabled = false;
				sell.Disabled = false;
			}
			else
			{
				shipSelection.AddItem("None");
				shipSelection.Selected = 0;
			}

			UpdateMaxItems();
		},
		TaskScheduler.FromCurrentSynchronizationContext());

		UpdateCost();
	}

	private async Task<List<Ship>> UpdateShips()
	{
		var ships = new List<Ship>();

		await foreach (var ship in Data.Item2.GetShips())
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

			amount.MaxValue = ship.Cargo.Inventory.First(i => i.Symbol == Data.Item1.Symbol.ToString()).Units;
		}
		else amount.MaxValue = 0;

		amount.Suffix = $"/ {amount.MaxValue}";
	}

	public void UpdateCost()
	{
		price.Text = $"${amount.Value * Data.Item1.SellPrice}";
	}

	public void OpenShipUI()
	{
		ShipInfoNode.Show(ships[shipSelection.Selected]);
	}

	public void Sell()
	{
		sell.Disabled = true;
		sell.Text = "Selling...";

		ships[shipSelection.Selected].SellCargo(Data.Item1.Symbol.ToString(), (int)amount.Value).ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to sell. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}

			sell.Text = "Sell";
			sell.Disabled = false;

			Close();
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	public void Close()
	{
		UINode.Instance.HidePrevUI();
	}
}

