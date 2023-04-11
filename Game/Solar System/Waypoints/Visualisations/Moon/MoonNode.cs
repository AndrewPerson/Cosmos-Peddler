using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class MoonNode : WaypointVisualiser
{
	public override Vector3 OrbitCentre { get; set; }
	public override Waypoint Waypoint { get; set; } = null!;
	
	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
		
		GetNode<MeshInstance3D>("%Mesh").SetInstanceShaderParameter("seed", seed);

		GetNode<AnimationPlayer>("%Rotation").Play("Rotate");
	}
}
