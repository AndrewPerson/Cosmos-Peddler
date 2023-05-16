using Godot;
using CosmosPeddler.Game.UI.ShipInfo;
using CosmosPeddler.Game.UI.Navigate;

namespace CosmosPeddler.Game.UI.WaypointInfo;

public partial class ShipNode : ReactiveUI<Ship>
{
	private Label name = null!;
    private Button navigate = null!;
    private Button orbitToggle = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
        navigate = GetNode<Button>("%Navigate");
        orbitToggle = GetNode<Button>("%Orbit Toggle");
	}

	public override void UpdateUI()
	{
		name.Text = Data.Symbol.ToHuman();
		name.TooltipText = Data.Symbol;

        navigate.Disabled = Data.Nav.Status != ShipNavStatus.IN_ORBIT;

        orbitToggle.Disabled = false;

        if (Data.Nav.Status == ShipNavStatus.IN_ORBIT)
        {
            orbitToggle.Text = "Dock";
            orbitToggle.Pressed += Dock;
        }
        else if (Data.Nav.Status == ShipNavStatus.DOCKED)
        {
            orbitToggle.Text = "Orbit";
            orbitToggle.Pressed += Orbit;
        }
        else
        {
            orbitToggle.Text = "In transit";
            orbitToggle.Disabled = true;
        }
	}

	public void ShowShipInfo()
	{
		ShipInfoNode.Show(Data);
	}

    public void NavigateShip()
    {
        NavigateNode.Show(Data);
    }

    public void Orbit()
    {
        orbitToggle.Text = "Orbiting...";
        orbitToggle.Disabled = true;

        Data.Orbit().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                ThreadSafe.Run(() => PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to orbit ship. Check the log for errors."));
                Logger.Error(t.Exception?.ToString() ?? "Unknown error");
            }

            orbitToggle.Pressed -= Orbit;
            UpdateUI();
        });
    }

    public void Dock()
    {
        orbitToggle.Text = "Docking...";
        orbitToggle.Disabled = true;

        Data.Dock().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                ThreadSafe.Run(() => PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to dock ship. Check the log for errors."));
                Logger.Error(t.Exception?.ToString() ?? "Unknown error");
            }

            orbitToggle.Pressed -= Dock;
            UpdateUI();
        });
    }
}
