using Godot;

namespace CosmosPeddler.Game.SolarSystem.Stars.OrangeStar;

public partial class OrangeStarNode : MeshInstance3D, IDimensionedObject
{
	public Vector3 Dimensions { get; private set; } = new Vector3(1, 1, 1);

	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
		SetInstanceShaderParameter("Seed", seed);
	}
}
