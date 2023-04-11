using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class JumpGateNode : WaypointVisualiser
{
	public override Vector3 OrbitCentre { get; set; }
	public override Waypoint Waypoint { get; set; } = null!;

	public override void _Ready()
	{
		LookAt(OrbitCentre);
	}
}
