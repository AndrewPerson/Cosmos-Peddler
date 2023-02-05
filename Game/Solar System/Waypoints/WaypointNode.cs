using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosPeddler.Game.UI;
using CosmosPeddler.Game.UI.WaypointInfo;
using CosmosPeddler.Game.SolarSystem.OrbitIndicator;

namespace CosmosPeddler.Game.SolarSystem.Waypoints;

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
	private Node3D indicators = null!;
	private Sprite3D shipsIndicator = null!;
	private Sprite3D marketIndicator = null!;
	private Sprite3D shipyardIndicator = null!;
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
		indicators = GetNode<Node3D>("%Indicators");
		shipsIndicator = GetNode<Sprite3D>("%Ships Indicator");
		marketIndicator = GetNode<Sprite3D>("%Market Indicator");
		shipyardIndicator = GetNode<Sprite3D>("%Shipyard Indicator");
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
		waypointVisualiser.OrbitCentre = orbitalTarget == null ? solarSystemCenter : orbitalTarget.GlobalPosition;

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
			
			var startingPosition = new Vector3(orbitalTarget.dimensions.X, 0, orbitalTarget.dimensions.Z) / 2 + dimensions / 2;

			Position = orbitalTarget.Position + startingPosition.Rotated(new Vector3(0, 1, 0), Mathf.DegToRad(rotation));
		}
#endregion

#region Orbit Indicator
		if (orbitalTarget == null)
		{
			orbitIndicator.GlobalPosition = solarSystemCenter;
			orbitIndicator.GlobalRotation = new Vector3(0, Mathf.Atan2(GlobalPosition.X - solarSystemCenter.X, GlobalPosition.Z - solarSystemCenter.Z), 0);

			orbitIndicator.OrbitRadius = solarSystemCenter.DistanceTo(new Vector3(GlobalPosition.X, 0, GlobalPosition.Z));
		}
		else
		{
			orbitIndicator.GlobalPosition = orbitalTarget.GlobalPosition;
			orbitIndicator.GlobalRotation = new Vector3(0, Mathf.Atan2(GlobalPosition.X - orbitalTarget.GlobalPosition.X, GlobalPosition.Z - orbitalTarget.GlobalPosition.Z), 0);

			orbitIndicator.OrbitRadius = new Vector3(orbitalTarget.GlobalPosition.X, 0, orbitalTarget.GlobalPosition.Z).DistanceTo(new Vector3(GlobalPosition.X, 0, GlobalPosition.Z));			
		}

		orbitIndicator.Scale /= Scale;
#endregion

#region Name
		name.Text = waypoint.Symbol;
		name.Position = new Vector3(0, new Vector2(dimensions.Z / 2, dimensions.Y / 2).Length(), 0) / Scale;
		name.Scale /= Scale;
#endregion

#region Indicators
		indicators.Position = new Vector3(0, -new Vector2(dimensions.Z / 2, dimensions.Y / 2).Length(), 0) / Scale;
		indicators.Scale /= Scale;

		SetIndicators(
			ships: false,
			market: waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.MARKETPLACE),
			shipyard: waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.SHIPYARD)
		);

		UINode.Instance.UIAppear += HideIndicators;
		UINode.Instance.UIDisappear += ShowIndicators;

		waypoint.HasShips().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to fetch ships. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}

			SetIndicators(
				ships: t.Result,
				market: waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.MARKETPLACE),
				shipyard: waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.SHIPYARD)
			);
		},
		TaskScheduler.FromCurrentSynchronizationContext());
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
					UINode.Instance.UIDisappear += EndUIZoom;

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

	public void EndUIZoom()
	{
		UINode.Instance.UIDisappear -= EndUIZoom;
		GameCameraNode.Instance.EndZoom();
		focused = false;
	}

	public void ShowIndicators() => indicators.Visible = true;
	public void HideIndicators() => indicators.Visible = false;

	public void SetIndicators(bool ships = false, bool market = false, bool shipyard = false)
	{
		List<Sprite3D> activeIndicators = new();
		
		if (ships)
		{
			activeIndicators.Add(shipsIndicator);
			shipsIndicator.Visible = true;
		}
		else shipsIndicator.Visible = false;

		if (market)
		{
			activeIndicators.Add(marketIndicator);
			marketIndicator.Visible = true;
		}
		else marketIndicator.Visible = false;

		if (shipyard)
		{
			activeIndicators.Add(shipyardIndicator);
			shipyardIndicator.Visible = true;
		}
		else shipyardIndicator.Visible = false;

		if (activeIndicators.Count > 0)
		{
			var indicatorWidth = activeIndicators.Sum(x => x.Texture.GetSize().X) - activeIndicators[0].Texture.GetSize().X / 2 - activeIndicators[^1].Texture.GetSize().X / 2;
			var indicatorOffset = indicatorWidth / -2;

			foreach (var indicator in activeIndicators)
			{
				indicator.Offset = new Vector2(indicatorOffset, indicator.Offset.Y);
				indicatorOffset += indicator.Texture.GetSize().X;
			}
		}
	}

    public override void _ExitTree()
    {
        if (focused)
		{
			UINode.Instance.UIDisappear -= EndUIZoom;
			GameCameraNode.Instance.EndZoom();
		}

		UINode.Instance.UIAppear -= HideIndicators;
		UINode.Instance.UIDisappear -= ShowIndicators;
    }
}
