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

		Waypoint.GetShips().ContinueWith(task =>
		{
			if (task.IsFaulted)
			{
				GD.PrintErr(task.Exception);
				return;
			}

			var ships = task.Result;

			if (ships.Length == 0)
			{
				SetStatus("No ships");
				return;
			}

			ClearStatus();

			for (int i = 0; i < Mathf.Min(shipsList.GetChildCount(), ships.Length); i++)
			{
				var ship = shipsList.GetChild<ShipNode>(i);
				ship.Ship = ships[i];
			}

			for (int i = Mathf.Min(shipsList.GetChildCount(), ships.Length); i < ships.Length; i++)
			{
				var ship = shipScene.Instantiate<ShipNode>();
				ship.Ready += () => ship.Ship = ships[i];

				shipsList.AddChild(ship);
			}

			for (int i = ships.Length; i < shipsList.GetChildCount(); i++)
			{
				shipsList.GetChild(i).QueueFree();
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
