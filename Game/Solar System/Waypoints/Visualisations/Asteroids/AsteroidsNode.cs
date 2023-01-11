using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class AsteroidsNode : GPUParticles3D, IWaypointVisualiser
{
	public Vector3 Dimensions { get; private set; } = new Vector3(1, 1, 1);

	public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;
}
