using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class UnknownNode : WaypointVisualiser
{
	public override Vector3 OrbitCentre { get; set; }
    public override Waypoint Waypoint { get; set; } = null!;
}
