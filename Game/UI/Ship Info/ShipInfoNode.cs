using Godot;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class ShipInfoNode : PopupUI<Ship>
{
	private Label name = null!;
	private FuelNode fuel = null!;
    private CargoNode cargo = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		fuel = GetNode<FuelNode>("%Fuel");
        cargo = GetNode<CargoNode>("%Cargo");
	}

	public override void UpdateUI()
	{
		if (Data.Symbol == Data.Registration.Name) name.Text = $"{Data.Symbol} - {Data.Registration.Role}";
		else name.Text = $"{Data.Symbol} ({Data.Registration.Name}) - {Data.Registration.Role}";

		fuel.Data = Data;
        cargo.Data = Data;
	}
}
