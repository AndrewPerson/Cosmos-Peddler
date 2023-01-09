using Godot;
using System.Linq;

namespace CosmosPeddler.Game;

public partial class CargoNode : ReactiveUI<(ShipNav, ShipCargo)>
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
		var (nav, cargo) = Data;

		header.Text = $"Cargo - {cargo.Units}/{cargo.Capacity}";

		var items = cargo.Inventory.ToArray();

		RenderList(cargoItems, cargoItemScene, items.Select(i => (nav, i)).ToArray());
	}
}
