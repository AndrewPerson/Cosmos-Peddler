using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class JumpGateNode : MeshInstance3D, IWaypointVisualiser
{
    public Vector3 Dimensions { get; private set; } = new Vector3(0.5f, 0.5f, 0.2f);

    public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;

    public override void _Ready()
    {
        LookAt(OrbitCentre);
    }
}
