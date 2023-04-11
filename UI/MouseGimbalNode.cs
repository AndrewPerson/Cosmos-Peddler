using Godot;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class MouseGimbalNode : Node3D
{
	[Export]
	public float MouseSpeed { get; set; } = 0.01f;

	[Export]
	public float KeyboardSpeed { get; set; } = 0.1f;

	public override void _Process(double delta)
	{
		float right = Input.GetActionStrength("right") - Input.GetActionStrength("left");

		Rotation += new Vector3(0, -right * KeyboardSpeed, 0);
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			if (@event is InputEventMouseMotion mouseMotion)
			{
				Rotation += new Vector3(0, -mouseMotion.Relative.X * MouseSpeed, 0);
			}
		}
	}
}
