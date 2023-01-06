using Godot;

namespace CosmosPeddler.Game;

public partial class AsteroidsNode : GPUParticles3D, IWaypointVisualiser
{
	public Vector3 Dimensions { get; private set; } = new Vector3(1, 1, 1);

	public Vector3 SolarSystemCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;
}
