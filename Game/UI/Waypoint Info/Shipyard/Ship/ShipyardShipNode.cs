using Godot;

namespace CosmosPeddler.Game;

public partial class ShipyardShipNode : ReactiveUI<ShipyardShip>
{
	private Label name = null!;
	private Button buy = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		buy = GetNode<Button>("%Buy");
	}

	public override void UpdateUI()
	{
		name.Text = Data.Name.Replace('_', ' ');
		name.TooltipText = Data.Description;

		buy.Text = $"Buy - ${Data.PurchasePrice}";
	}
}
