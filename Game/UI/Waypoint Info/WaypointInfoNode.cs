using Godot;
using System.Linq;

namespace CosmosPeddler.Game;

public partial class WaypointInfoNode : Control
{
	private static WaypointInfoNode Instance { get; set; } = null!;

	private Waypoint _waypoint = null!;

	public Waypoint Waypoint
	{
		get => _waypoint;
		set
		{
			_waypoint = value;
			UpdateWaypointInfo();
		}
	}

	private Label name = null!;
	private Label extraInfo = null!;
	private TraitsNode traits = null!;
	private ShipsNode ships = null!;
	private ShipyardNode shipyard = null!;
	private MarketNode market = null!;

	public static void Show(Waypoint waypoint)
	{
		Instance.Waypoint = waypoint;
		UINode.Instance.Show(Instance);
	}

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		extraInfo = GetNode<Label>("%Extra Info");

		traits = GetNode<TraitsNode>("%Traits");
		ships = GetNode<ShipsNode>("%Ships");
		shipyard = GetNode<ShipyardNode>("%Shipyard");
		market = GetNode<MarketNode>("%Market");
	}

	public override void _Ready()
	{
		Instance = this;
	}

	private void UpdateWaypointInfo()
	{
		name.Text = Waypoint.Symbol;
		extraInfo.Text = $"{Waypoint.Type.ToString().Replace('_', ' ')} - {Waypoint.Faction?.Symbol.Replace('_', ' ') ?? "NO FACTION"}";

		traits.Traits = Waypoint.Traits.ToArray();
		ships.Waypoint = Waypoint;
		shipyard.Waypoint = Waypoint;
		market.Waypoint = Waypoint;
	}
}
