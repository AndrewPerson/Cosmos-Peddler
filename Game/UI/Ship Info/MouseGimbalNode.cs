using Godot;

namespace CosmosPeddler.Game.UI.ShipInfo;

public partial class MouseGimbalNode : Node3D
{
	[Export]
	public float mouseSpeed = 0.01f;

	[Export]
	public float keyboardSpeed = 0.1f;

	public override void _Process(double delta)
	{
		float right = Input.GetActionStrength("right") - Input.GetActionStrength("left");

		Rotation += new Vector3(0, -right * keyboardSpeed, 0);
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			if (@event is InputEventMouseMotion mouseMotion)
			{
				var viewport = GetViewport();

				if (viewport.GetVisibleRect().HasPoint(mouseMotion.Position))
				{
					Rotation += new Vector3(0, -mouseMotion.Relative.x * mouseSpeed, 0);
				}
			}
		}
	}
}
