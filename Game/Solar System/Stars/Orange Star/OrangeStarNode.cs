using Godot;

namespace CosmosPeddler.Game.SolarSystem.Stars.OrangeStar;

public partial class OrangeStarNode : CollisionShape3D
{
	public override void _Ready()
	{
		var seed = (float)GD.RandRange(0f, 10f);
		GetNode<MeshInstance3D>("%Mesh").SetInstanceShaderParameter("seed", seed);
	}
}
