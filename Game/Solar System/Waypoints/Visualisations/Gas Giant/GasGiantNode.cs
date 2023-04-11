using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class GasGiantNode : WaypointVisualiser
{
	public override Vector3 OrbitCentre { get; set; }
    public override Waypoint Waypoint { get; set; } = null!;

    private MeshInstance3D body = null!;
	private MeshInstance3D rings = null!;

    public override void _EnterTree()
    {
        body = GetNode<MeshInstance3D>("%Body");
		rings = GetNode<MeshInstance3D>("%Rings");
    }

	public override void _Ready()
	{
        LookAt(OrbitCentre);

		var seed = (float)GD.RandRange(0f, 10f);
        SetSeed(seed);
	}

	private void SetSeed(float seed)
	{
		body.SetInstanceShaderParameter("seed", seed);
		rings.SetInstanceShaderParameter("seed", seed);
	}
}
