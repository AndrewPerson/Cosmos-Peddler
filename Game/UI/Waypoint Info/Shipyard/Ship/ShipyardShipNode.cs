using Godot;

namespace CosmosPeddler.Game;

public partial class ShipyardShipNode : Control
{
	private ShipyardShip _ship = null!;

	public ShipyardShip Ship
	{
		get => _ship;
		set
		{
			_ship = value;
			UpdateShipInfo();
		}
	}

	private Label name = null!;
	private Button buy = null!;

	public void UpdateShipInfo()
	{
		name.Text = Ship.Name.Replace('_', ' ');
		name.TooltipText = Ship.Description;

		buy.Text = $"Buy - ${Ship.PurchasePrice}";
	}
}
