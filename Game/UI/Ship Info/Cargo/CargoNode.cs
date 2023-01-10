using Godot;
using System.Linq;

namespace CosmosPeddler.Game;

public partial class CargoNode : ReactiveUI<(ShipCargo, ShipNav)>
{
	[Export]
	public PackedScene cargoItemScene = null!;

	private Label header = null!;
	private Node cargoItems = null!;

	public override void _EnterTree()
	{
		header = GetNode<Label>("%Header");
		cargoItems = GetNode<Node>("%Cargo List");
	}

    public override void UpdateUI()
	{
		var (cargo, nav) = Data;

		header.Text = $"Cargo - {cargo.Units}/{cargo.Capacity}";

		var items = cargo.Inventory.ToArray();

		RenderList(cargoItems, cargoItemScene, items.Select(i => (i, nav)).ToArray());
	}
}
