using Godot;

namespace CosmosPeddler.Game;

public partial class OrbitalStationNode : MeshInstance3D, IWaypointVisualiser
{
	public Vector3 Dimensions { get; private set; } = new Vector3(1.5f, 0.2f, 0.4f);

	public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;

	public override void _Ready()
	{
		CallDeferred("look_at", OrbitCentre);
	}
}