using Godot;

namespace CosmosPeddler.Game.SolarSystem.Stars.Blackhole;

public partial class BlackholeNode : MeshInstance3D, IDimensionedObject
{
	[Export]
	public Vector3 Dimensions { get; private set; } = new Vector3(1, 1, 1);

	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
		SetInstanceShaderParameter("seed", seed);
	}

	public override void _Process(double delta)
	{
		var dist = Position.DistanceTo(GetViewport().GetCamera3d().Position);

		float brightness = 3 * Mathf.Log(Mathf.Max(dist * 2, 3));

		SetInstanceShaderParameter("emission_strength", brightness);
	}
}
