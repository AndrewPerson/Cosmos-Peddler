using Godot;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosPeddler.Game.UI.WaypointInfo;

public partial class ShipyardNode : ReactiveUI<Waypoint>
{
	[Export]
	public PackedScene ShipScene { get; set; } = null!;

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

	public override void UpdateUI()
	{
		if (!Data.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.SHIPYARD))
		{
			SetStatus("No shipyard");
			return;
		}

		SetStatus("Loading...");

		Data.GetShipyard().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				SetStatus("Failed to fetch shipyard. Check the log for errors.");
				return;
			}

			var shipyard = t.Result;

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

			RenderList(shipsList, ShipScene, ships);
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
