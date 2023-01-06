using Godot;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class ShipyardNode : PanelContainer
{
	private Waypoint _waypoint = null!;

	public Waypoint Waypoint
	{
		get => _waypoint;
		set
		{
			_waypoint = value;
			UpdateShipyardInfo();
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

	private void UpdateShipyardInfo()
	{
		if (!Waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.SHIPYARD))
		{
			SetStatus("No shipyard");
			return;
		}

		SetStatus("Loading...");

		Waypoint.GetShipyard().ContinueWith(task =>
		{
			if (task.IsFaulted)
			{
				GD.PrintErr(task.Exception);
				return;
			}

			var shipyard = task.Result;

			if (shipyard == null)
			{
				SetStatus("No shipyard");
				return;
			}

			if (shipyard.Ships == null)
			{
				SetStatus("No ship present");
				return;
			}

			var ships = shipyard.Ships.ToArray();

			if (ships.Length == 0)
			{
				SetStatus("No ships");
				return;
			}

			ClearStatus();

			for (int i = 0; i < Mathf.Min(shipsList.GetChildCount(), ships.Length); i++)
			{
				var ship = shipsList.GetChild<ShipyardShipNode>(i);
				ship.Ship = ships[i];
			}

			for (int i = Mathf.Min(shipsList.GetChildCount(), ships.Length); i < ships.Length; i++)
			{
				var ship = shipScene.Instantiate<ShipyardShipNode>();
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
