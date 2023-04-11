using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class AsteroidsNode : WaypointVisualiser
{
	public override Vector3 OrbitCentre { get; set; }
    public override Waypoint Waypoint { get; set; } = null!;
}
