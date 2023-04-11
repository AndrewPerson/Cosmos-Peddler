using Godot;
using CosmosPeddler.Game.UI.ShipInfo;

namespace CosmosPeddler.Game.UI.WaypointInfo;

public partial class ShipNode : ReactiveUI<Ship>
{
	private Label name = null!;
    private Button navigate = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
        navigate = GetNode<Button>("%Navigate");
	}

	public override void UpdateUI()
	{
		name.Text = Data.Symbol.ToHuman();
		name.TooltipText = Data.Symbol;

        navigate.Disabled = Data.Nav.Status != ShipNavStatus.IN_ORBIT;
	}

	public void ShowShipInfo()
	{
		ShipInfoNode.Show(Data);
	}
}
