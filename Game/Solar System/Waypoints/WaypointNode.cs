using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CosmosPeddler.Game;

public partial class WaypointNode : Node3D
{
	private static Dictionary<string, PackedScene>? waypointTypes = null;

	public Waypoint waypoint = null!;
	public Vector3 solarSystemCenter;
	public float mapScale = 1.0f;
	public WaypointNode? orbitalTarget;

	public Vector3 dimensions;

	[Export]
	public StringName[] waypointTypeNames = Array.Empty<StringName>();

	[Export]
	public PackedScene[] waypointTypeScenes = Array.Empty<PackedScene>();

	private bool hovering = false;
	private bool focused = false;

	private OrbitIndicatorNode orbitIndicator = null!;
	private Label3D name = null!;
	private CollisionShape3D bounds = null!;

	public override void _EnterTree()
	{
#region Generate Waypoint Types
		if (waypointTypes == null)
		{
			waypointTypes = new Dictionary<string, PackedScene>();

			lock(waypointTypes)
			{
				for (int i = 0; i < waypointTypeNames.Length; i++)
				{
					waypointTypes.Add(waypointTypeNames[i], waypointTypeScenes[i]);
				}
			}
		}
#endregion

#region Get Nodes
		orbitIndicator = GetNode<OrbitIndicatorNode>("%Orbit Indicator");
		name = GetNode<Label3D>("%Name");
		bounds = GetNode<CollisionShape3D>("%Bounds");
#endregion
	}

    public override void _Ready()
	{
#region Create Waypoint
		if (orbitalTarget != null) Scale = new Vector3(0.5f, 0.5f, 0.5f);

		Node3D waypointInstance;
		if (waypointTypes!.TryGetValue(waypoint.Type.ToString(), out var scene))
		{
			waypointInstance = scene.Instantiate<Node3D>();
		}
		else
		{
			waypointInstance = waypointTypes["UNKNOWN"].Instantiate<Node3D>();
		}

		var waypointVisualiser = (IWaypointVisualiser)waypointInstance;

		waypointVisualiser.Waypoint = waypoint;
		waypointVisualiser.SolarSystemCentre = solarSystemCenter;

		waypointInstance.Visible = false;
		Ready += () => waypointInstance.SetDeferred("visible", true);

		AddChild(waypointInstance);

		dimensions = waypointVisualiser.Dimensions * Scale;
#endregion

#region Create Bounds
		((BoxShape3D)bounds.Shape).Size = dimensions / Scale;
#endregion

#region Position (And scale if in orbit)
		if (orbitalTarget == null)
		{
			Position = new Vector3(waypoint.X, 0, waypoint.Y) * mapScale;
		}
		else
		{
			var rotation = 360 * orbitalTarget.waypoint.Orbitals.Select(x => x.Symbol).ToList().IndexOf(waypoint.Symbol) / (orbitalTarget.waypoint.Orbitals.Count < 3 ? 3 : orbitalTarget.waypoint.Orbitals.Count);
			
			var startingPosition = new Vector3(orbitalTarget.dimensions.x, 0, orbitalTarget.dimensions.z) / 2 + dimensions / 2;

			Position = orbitalTarget.Position + startingPosition.Rotated(new Vector3(0, 1, 0), Mathf.DegToRad(rotation));
		}
#endregion

#region Orbit Indicator
		if (orbitalTarget == null)
		{
			orbitIndicator.GlobalPosition = solarSystemCenter;
			orbitIndicator.GlobalRotation = new Vector3(0, Mathf.Atan2(GlobalPosition.x - solarSystemCenter.x, GlobalPosition.z - solarSystemCenter.z), 0);

			orbitIndicator.OrbitRadius = solarSystemCenter.DistanceTo(new Vector3(GlobalPosition.x, 0, GlobalPosition.z));
		}
		else
		{
			orbitIndicator.GlobalPosition = orbitalTarget.GlobalPosition;
			orbitIndicator.GlobalRotation = new Vector3(0, Mathf.Atan2(GlobalPosition.x - orbitalTarget.GlobalPosition.x, GlobalPosition.z - orbitalTarget.GlobalPosition.z), 0);

			orbitIndicator.OrbitRadius = new Vector3(orbitalTarget.GlobalPosition.x, 0, orbitalTarget.GlobalPosition.z).DistanceTo(new Vector3(GlobalPosition.x, 0, GlobalPosition.z));			
		}

		orbitIndicator.Scale /= Scale;
#endregion

#region Name
		name.Text = waypoint.Symbol;
		name.Position = new Vector3(0, dimensions.y / 2, -dimensions.z / 2) / Scale;
		name.Scale /= Scale;
#endregion
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				if (hovering)
				{
					focused = true;
					GameCameraNode.Instance.ZoomTo(GlobalPosition, dimensions);
					UINode.Instance.AllUICleared += GameCameraNode.Instance.EndZoom;
					UINode.Instance.AllUICleared += () => UINode.Instance.AllUICleared -= GameCameraNode.Instance.EndZoom;

					WaypointInfoNode.Show(waypoint);

					MouseExit();
				}
			}
		}
	}

	public void MouseEnter()
	{
		hovering = true;
		ShowOrbit();
	}

	public void MouseExit()
	{
		hovering = false;
		HideOrbit();
	}

	public void ShowOrbit()
	{
		orbitIndicator.Visible = true;
		name.Visible = true;

		orbitalTarget?.ShowOrbit();
	}

	public void HideOrbit()
	{
		orbitIndicator.Visible = false;
		name.Visible = false;

		orbitalTarget?.HideOrbit();
	}
}
