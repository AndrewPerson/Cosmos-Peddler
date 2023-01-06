using Godot;

namespace CosmosPeddler.Game;

public partial class ShipNode : HBoxContainer
{
	private Ship _ship = null!;

	public Ship Ship
	{
		get => _ship;
		set
		{
			_ship = value;
			UpdateShipInfo();
		}
	}

	private Label name = null!;
	
	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
	}

	private void UpdateShipInfo()
	{
		name.Text = Ship.Symbol.Replace('_', ' ');
		name.TooltipText = Ship.Symbol;
	}

	public void ShowShipInfo()
	{
		ShipInfoNode.Show(Ship);
	}
}
