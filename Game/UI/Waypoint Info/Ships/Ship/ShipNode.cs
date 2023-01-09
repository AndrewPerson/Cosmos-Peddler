using Godot;

namespace CosmosPeddler.Game;

public partial class ShipNode : ReactiveUI<Ship>
{
	private Label name = null!;
	
	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
	}

	public override void UpdateUI()
	{
		name.Text = Data.Symbol.Replace('_', ' ');
		name.TooltipText = Data.Symbol;
	}

	public void ShowShipInfo()
	{
		ShipInfoNode.Show(Data);
	}
}
