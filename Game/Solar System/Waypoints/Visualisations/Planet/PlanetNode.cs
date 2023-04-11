using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class PlanetNode : WaypointVisualiser
{
	public override Vector3 OrbitCentre { get; set; }
	public override Waypoint Waypoint { get; set; } = null!;

    private MeshInstance3D land = null!;
	private MeshInstance3D ocean = null!;

    public override void _EnterTree()
    {
        land = GetNode<MeshInstance3D>("%Land");
        ocean = GetNode<MeshInstance3D>("%Ocean");
    }

	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
		SetSeed(seed);

		GetNode<AnimationPlayer>("%Rotation").Play("Rotate");
	}

	private void SetSeed(float seed)
	{
		land.SetInstanceShaderParameter("seed", seed);
		ocean.SetInstanceShaderParameter("seed", seed);
	}
}
