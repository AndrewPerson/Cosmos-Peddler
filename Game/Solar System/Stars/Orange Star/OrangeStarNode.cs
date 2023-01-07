using Godot;

namespace CosmosPeddler.Game;

public partial class OrangeStarNode : MeshInstance3D, IDimensionedObject
{
	[Export]
	public Vector3 Dimensions { get; private set; } = new Vector3(1, 1, 1);

	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
        SetSeed(seed);
	}

	public override void _Process(double delta)
	{
		var dist = Position.DistanceTo(GetViewport().GetCamera3d().Position);

		float brightness = 3 * Mathf.Log(Mathf.Max(dist / 2, 3));

		SetInstanceShaderParameter("Glow_Power", brightness);
	}

	private void SetSeed(float seed)
	{
		SetInstanceShaderParameter("Seed", seed);
	}
}
