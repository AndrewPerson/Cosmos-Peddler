using Godot;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class FuelNode : ReactiveUI<Ship>
{
	private Label name = null!;
	private ProgressBar fuelLevel = null!;
	private Button refuel = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		fuelLevel = GetNode<ProgressBar>("%Fuel Level");
		refuel = GetNode<Button>("%Refuel");
	}

	public override void UpdateUI()
	{
		var fuel = Data.Fuel;
		name.Text = $"Fuel - {fuel.Current}/{fuel.Capacity}";

		fuelLevel.MaxValue = fuel.Capacity;
		fuelLevel.Value = fuel.Current;
	}

	public void Refuel()
	{
		refuel.Text = "Refuelling...";
		refuel.Disabled = true;

		Data.Refuel().ContinueWith(t =>
		{
			refuel.Text = "Refuel";
			refuel.Disabled = false;

			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, $"Failed to refuel ship. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}
		});
	}
}
