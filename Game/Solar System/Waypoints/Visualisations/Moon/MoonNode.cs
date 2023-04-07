using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class MoonNode : MeshInstance3D, IWaypointVisualiser
{
	public Vector3 Dimensions { get; private set; } = new Vector3(0.5f, 0.5f, 0.5f);

	public Vector3 OrbitCentre { get; set; }
	public Waypoint Waypoint { get; set; } = null!;
	
	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
		
		SetInstanceShaderParameter("seed", seed);

		GetNode<AnimationPlayer>("%Rotation").Play("Rotate");
	}
}
