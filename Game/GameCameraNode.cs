using Godot;

namespace CosmosPeddler.Game;

public partial class GameCameraNode : Camera3D
{
	public static GameCameraNode Instance { get; private set; } = null!;

	[Export]
	public float speed = 500;

	[Export]
	public float mouseSpeed = 1;

	[Export]
	public float zoomSpeed = 1;

	[Export]
	public float minZoom = 1;

	[Export]
	public float maxZoom = 5;

	private Vector3 preZoomPosition;
	private Vector3 preZoomRotation;

	public Vector2 mapPosition;
	private float zoom;

	private Tween? zoomTween;

	private bool forcedZoom = false;

	//Magic number to determine % of screen taken up by object when zooming in.
	//Don't know how it actually relates to % of screen taken up.
	private const float objectZoomSize = 1.5f;

	override public void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		zoom = maxZoom;
		mapPosition = new Vector2(Position.x, Position.z);

		preZoomRotation = GlobalRotation;
	}

	public override void _Process(double delta)
	{
		if (forcedZoom) return;

		float forwards = Input.GetActionStrength("backwards") - Input.GetActionStrength("forwards");
		float right = Input.GetActionStrength("right") - Input.GetActionStrength("left");

		mapPosition += new Vector2(right, forwards) * (float)delta * speed * zoom;

		Position = new Vector3(mapPosition.x, 0, mapPosition.y) + Transform.basis.z * zoom;
	}

	private Vector3? GetMouseWorldPosition(Vector2 mouse)
	{
		var plane = new Plane(Vector3.Up, 0);

		var from = ProjectRayOrigin(mouse);
		var dir = ProjectRayNormal(mouse);

		return plane.IntersectRay(from, dir);
	}

	public override void _Input(InputEvent @event)
	{
		if (forcedZoom) return;

		if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			if (@event is InputEventMouseMotion mouseMotion)
			{
				var currentWorldPosition = GetMouseWorldPosition(mouseMotion.Position);
				var oldWorldPosition = GetMouseWorldPosition(mouseMotion.Position - mouseMotion.Relative);

				if (currentWorldPosition.HasValue && oldWorldPosition.HasValue)
				{
					var delta = currentWorldPosition.Value - oldWorldPosition.Value;

					mapPosition -= new Vector2(delta.x * mouseSpeed, delta.z * mouseSpeed);
				}
			}
		}
		else if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.IsPressed())
			{
				float zoomMovement = (mouseButton.ButtonIndex == MouseButton.WheelUp ? -1 :
							 mouseButton.ButtonIndex == MouseButton.WheelDown ? 1 :
							 0) * zoomSpeed;

				zoom = Mathf.Clamp(zoomMovement + zoom, minZoom, maxZoom);
			}
		}
	}

	public void ZoomTo(Vector3 position, Vector3 dimensions)
	{
		if (!forcedZoom) preZoomPosition = GlobalPosition;

		forcedZoom = true;

		zoomTween?.Kill();
		zoomTween?.Dispose();

		var size = Mathf.Sqrt(dimensions.x * dimensions.x + dimensions.y * dimensions.y + dimensions.z * dimensions.z);

		var distance = size / objectZoomSize / Mathf.Tan(Mathf.DegToRad(Fov / 2));
		var finalPosition = position + Quaternion.FromEuler(new Vector3(
			Mathf.DegToRad(45),
			Mathf.DegToRad(180),
			0
		)) * Vector3.Forward * distance;

		var finalRotation = new Quaternion(new Transform3D(Quaternion.FromEuler(preZoomRotation), finalPosition).LookingAt(position, Vector3.Up).basis).GetEuler();

		zoomTween = CreateTween();

		zoomTween.Parallel().TweenProperty(this, "global_position", finalPosition, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);
		zoomTween.Parallel().TweenProperty(this, "global_rotation", finalRotation, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quart);
	}

	public void EndZoom()
	{
		zoomTween?.Kill();
		zoomTween?.Dispose();

		if (!forcedZoom) return;

		zoomTween = CreateTween();

		zoomTween.Parallel().TweenProperty(this, "global_position", preZoomPosition, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);
		zoomTween.Parallel().TweenProperty(this, "global_rotation", preZoomRotation, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart);

		zoomTween.Finished += () =>
		{
			forcedZoom = false;
		};
	}
}
