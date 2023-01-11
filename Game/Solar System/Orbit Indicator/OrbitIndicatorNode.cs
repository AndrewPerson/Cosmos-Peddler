using Godot;

namespace CosmosPeddler.Game.SolarSystem.OrbitIndicator;

[Tool]
public partial class OrbitIndicatorNode : Node3D
{
	private float _orbitRadius = 1;

	[Export]
	public float OrbitRadius
	{
		get => _orbitRadius;
		set
		{
			_orbitRadius = value;
			UpdateOrbitRadius();
		}
	}

	private MeshInstance3D orbitIndicator = null!;
	private MeshInstance3D waypointIndicator = null!;

	private bool ready = false;

	public override void _EnterTree()
	{
		orbitIndicator = GetNode<MeshInstance3D>("%Orbit");
		waypointIndicator = GetNode<MeshInstance3D>("%Waypoint Indicator");
	}

	public override void _Ready()
	{
		ready = true;

		UpdateOrbitRadius();
	}

	private void UpdateOrbitRadius()
	{
		if (!ready) return;

		#if TOOLS
		orbitIndicator ??= GetNode<MeshInstance3D>("%Orbit");
		waypointIndicator ??= GetNode<MeshInstance3D>("%Waypoint Indicator");
		#endif

		Scale = new Vector3(OrbitRadius, 1, OrbitRadius);

		orbitIndicator.SetInstanceShaderParameter("dash_count", Mathf.Round(10 * OrbitRadius));
		orbitIndicator.SetInstanceShaderParameter("line_width", 0.05f / OrbitRadius);
		orbitIndicator.SetInstanceShaderParameter("time_scale", 10 / OrbitRadius);

		waypointIndicator.SetInstanceShaderParameter("dash_count", 5 * OrbitRadius / Mathf.Pi);
		waypointIndicator.SetInstanceShaderParameter("line_width", 0.05f / OrbitRadius);
		waypointIndicator.SetInstanceShaderParameter("time_scale", 20f / 360f / OrbitRadius);
	}
}
