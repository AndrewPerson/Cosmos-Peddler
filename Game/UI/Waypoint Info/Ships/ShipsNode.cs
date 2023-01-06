using Godot;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class ShipsNode : PanelContainer
{
	private Waypoint _waypoint = null!;

	public Waypoint Waypoint
	{
		get => _waypoint;
		set
		{
			_waypoint = value;
			UpdateShipInfo();
		}
	}

	[Export]
	public PackedScene shipScene = null!;

	private Label status = null!;
	private Control shipsList = null!;

	public override void _EnterTree()
	{
		status = GetNode<Label>("%Status");
		shipsList = GetNode<Control>("%Ships List");
	}

	private void SetStatus(string text)
	{
		status.Text = text;
		status.Visible = true;
		shipsList.Visible = false;
	}

	private void ClearStatus()
	{
		status.Visible = false;
		shipsList.Visible = true;
	}

	private void UpdateShipInfo()
	{
		SetStatus("Loading...");

		UpdateShipInfoAsync().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				GD.PrintErr(t.Exception);
				SetStatus(t.Exception?.Message ?? "Unknown error");
				return;
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	private async Task UpdateShipInfoAsync()
	{
		for (int i = 0; i < shipsList.GetChildCount(); i++)
		{
			shipsList.GetChild(i).QueueFree();
		}

		bool noShips = true;
		await foreach (var ship in Waypoint.GetShips())
		{
			noShips = false;
			ClearStatus();

			var shipNode = shipScene.Instantiate<ShipNode>();
			shipNode.Ready += () => shipNode.Ship = ship;
			shipsList.AddChild(shipNode);
		}

		if (noShips)
		{
			SetStatus("No ships");
		}
	}
}
