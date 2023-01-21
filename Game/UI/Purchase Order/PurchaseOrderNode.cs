using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosPeddler.Game.UI.ShipInfo;

namespace CosmosPeddler.Game.UI.PurchaseOrder;

public partial class PurchaseOrderNode : DifferentiatedPopupUI<(MarketItem, Waypoint), PurchaseOrderNode>
{
	private bool loadedShips = false;
	private List<Ship> ships = null!;

	private Label name = null!;
	private OptionButton shipSelection = null!;
	private Button viewShip = null!;
	private SpinBox amount = null!;
	private Label price = null!;
	private Button purchase = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		shipSelection = GetNode<OptionButton>("%Ships");
		viewShip = GetNode<Button>("%View Ship");
		amount = GetNode<SpinBox>("%Amount");
		price = GetNode<Label>("%Price");
		purchase = GetNode<Button>("%Purchase");
	}

	public override void UpdateUI()
	{
		name.Text = $"Purchase {Data.Item1.Symbol.ToString().ToHuman()}";

		loadedShips = false;
		
		shipSelection.Clear();
		shipSelection.AddItem("Loading...");
		shipSelection.Disabled = true;
		shipSelection.Selected = 0;

		viewShip.Text = "Loading...";
		viewShip.Disabled = true;

		purchase.Text = "Loading...";
		purchase.Disabled = true;

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

				purchase.Text = "Purchase";
				purchase.Disabled = false;
			}
			else
			{
				shipSelection.AddItem("None");
				shipSelection.Selected = 0;

				viewShip.Text = "Unavailable";

				purchase.Text = "Unavailable";
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

			amount.MaxValue = ship.Cargo.Capacity - ship.Cargo.Units;
		}
		else amount.MaxValue = 0;

		amount.Suffix = $"/ {amount.MaxValue}";
	}

	public void UpdateCost()
	{
		price.Text = $"${(int)amount.Value * Data.Item1.PurchasePrice}";
	}

	public void OpenShipUI()
	{
		ShipInfoNode.Show(ships[shipSelection.Selected]);
	}

	public void Purchase()
	{
		purchase.Disabled = true;
		purchase.Text = "Purchasing...";

		ships[shipSelection.Selected].PurchaseCargo(Data.Item1.Symbol.ToString(), (int)amount.Value).ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				//TODO Show error
				GD.PrintErr(t.Exception);
				return;
			}

			purchase.Text = "Purchase";
			purchase.Disabled = false;

			Close();
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	public void Close()
	{
		UINode.Instance.HidePrevUI();
	}
}
