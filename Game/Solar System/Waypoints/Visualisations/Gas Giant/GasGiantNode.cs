using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class GasGiantNode : MeshInstance3D, IWaypointVisualiser
{
	public Vector3 Dimensions { get; private set; } = new Vector3(1, 0.5f, 1f);

	public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;

	private MeshInstance3D rings = null!;

	public override void _Ready()
	{
		rings = GetNode<MeshInstance3D>("%Rings");
        LookAt(OrbitCentre);

		var seed = (float)GD.RandRange(0f, 10f);
        SetSeed(seed);
	}

	private void SetSeed(float seed)
	{
		SetInstanceShaderParameter("seed", seed);
		rings.SetInstanceShaderParameter("seed", seed);
	}
}
