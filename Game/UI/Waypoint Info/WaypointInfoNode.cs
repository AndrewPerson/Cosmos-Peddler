using Godot;
using System.Linq;

namespace CosmosPeddler.Game.UI.WaypointInfo;

public partial class WaypointInfoNode : PopupUI<Waypoint, WaypointInfoNode>
{
	private Label name = null!;
	private Label extraInfo = null!;
	private TraitsNode traits = null!;
	private ShipsNode ships = null!;
	private ShipyardNode shipyard = null!;
	private MarketNode market = null!;

	public override void _EnterTree()
	{
		name = GetNode<Label>("%Name");
		extraInfo = GetNode<Label>("%Extra Info");

		traits = GetNode<TraitsNode>("%Traits");
		ships = GetNode<ShipsNode>("%Ships");
		shipyard = GetNode<ShipyardNode>("%Shipyard");
		market = GetNode<MarketNode>("%Market");
	}

	public override void UpdateUI()
	{
		name.Text = Data.Symbol;
		extraInfo.Text = $"{Data.Type.ToString().ToHuman()} - {Data.Faction?.Symbol.ToHuman() ?? "No Faction"}";

		traits.Data = Data.Traits.ToArray();
		ships.Data = Data;
		shipyard.Data = Data;
		market.Data = Data;
	}
}
