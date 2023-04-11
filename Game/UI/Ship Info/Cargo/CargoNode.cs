using Godot;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class CargoNode : ReactiveUI<Ship>
{
	private Label name = null!;
	private ProgressBar cargoUsage = null!;
	private Button view = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		cargoUsage = GetNode<ProgressBar>("%Cargo Usage");
		view = GetNode<Button>("%View");
	}

	public override void UpdateUI()
	{
		var cargo = Data.Cargo;
		name.Text = $"Cargo - {cargo.Units}/{cargo.Capacity}";

		cargoUsage.MaxValue = cargo.Capacity;
		cargoUsage.Value = cargo.Units;
	}

	public void ShowCargo()
	{
		CosmosPeddler.Game.UI.Cargo.CargoNode.Show((Data.Cargo, Data.Nav));
	}
}
