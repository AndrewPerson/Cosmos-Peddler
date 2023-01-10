using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class MarketOrderNode : ReactiveUI<(TradeSymbol, Market)>
{
	public MarketOrder? Order
	{
		get
		{
			if (ships == null || amount == null || !loadedShips)
				return null;

			return new MarketOrder()
			{
				ShipSymbol = shipSymbols[ships.Selected],
				Symbol = Data.Item1,
				Quantity = (int)amount.Value
			};
		}
	}

	private bool loadedShips = false;
	private List<string> shipSymbols = null!;

	private OptionButton ships = null!;
	private Button viewShip = null!;
	private SpinBox amount = null!;
	private Label cost = null!;

	public override void _EnterTree()
	{
		ships = GetNode<OptionButton>("%Ships");
		viewShip = GetNode<Button>("%View Ship");
		amount = GetNode<SpinBox>("%Amount");
		cost = GetNode<Label>("%Cost");
	}

	public override void UpdateUI()
	{
		loadedShips = false;
		
		ships.Clear();
		ships.AddItem("Loading...");
		ships.Disabled = true;
		ships.Selected = 0;

		UpdateShips().ContinueWith(t =>
		{
			loadedShips = true;
			ships.Clear();

			foreach (var ship in shipSymbols)
			{
				ships.AddItem(ship);
			}

			ships.Disabled = false;
			ships.Selected = 0;
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	private async Task UpdateShips()
	{
		shipSymbols = new();

		await foreach (var ship in SpaceTradersClient.GetMyShips())
		{
			shipSymbols.Add(ship.Symbol);
		}
	}
}
