using Godot;
using System.Linq;

namespace CosmosPeddler.Game.UI.Cargo;

public partial class CargoNode : PopupUI<(ShipCargo, ShipNav), CargoNode>
{
	[Export]
	public PackedScene CargoItemScene { get; set; } = null!;

	private Label name = null!;
	private Node cargoItems = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		cargoItems = GetNode<Node>("%Cargo Items");
	}

	public override void UpdateUI()
	{
		var (cargo, nav) = Data;

		name.Text = $"Cargo - {cargo.Units}/{cargo.Capacity}";

		var items = cargo.Inventory.ToArray();

		RenderList(cargoItems, CargoItemScene, items.Select(i => (i, nav)).ToArray());
	}
}
