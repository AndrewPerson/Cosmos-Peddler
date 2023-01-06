using Godot;
using System.Linq;

namespace CosmosPeddler.Game;

public partial class CargoNode : PanelContainer
{
	[Export]
	public PackedScene cargoItemScene = null!;

	private (ShipNav, ShipCargo) _cargoInfo = (null!, null!);

	public (ShipNav, ShipCargo) CargoInfo
	{
		get => _cargoInfo;
		set
		{
			_cargoInfo = value;
			UpdateCargo();
		}
	}

	private Label header = null!;
	private Node cargoItems = null!;

	public override void _EnterTree()
	{
		header = GetNode<Label>("%Header");
		cargoItems = GetNode<Node>("%Cargo List");
	}

	private void UpdateCargo()
	{
		var (nav, cargo) = CargoInfo;

		header.Text = $"Cargo - {cargo.Units}/{cargo.Capacity}";

		var items = cargo.Inventory.ToArray();

		for (int i = 0; i < Mathf.Min(cargoItems.GetChildCount(), items.Length); i++)
		{
			var cargoItem = cargoItems.GetChild<CargoItemNode>(i);
			cargoItem.CargoItemInfo = (nav, items[i]);
		}

		for (int i = Mathf.Min(cargoItems.GetChildCount(), items.Length); i < items.Length; i++)
		{
			var cargoItem = cargoItemScene.Instantiate<CargoItemNode>();
			cargoItem.Ready += () => cargoItem.CargoItemInfo = (nav, items[i]);

			cargoItems.AddChild(cargoItem);
		}

		for (int i = items.Length; i < cargoItems.GetChildCount(); i++)
		{
			cargoItems.GetChild(i).QueueFree();
		}
	}
}
