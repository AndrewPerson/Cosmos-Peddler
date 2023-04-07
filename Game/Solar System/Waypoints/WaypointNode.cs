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

	public Waypoint Waypoint { get; set; } = null!;
	public Vector3 SolarSystemCenter { get; set; }
	public WaypointNode? OrbitalTarget { get; set; }

	public Vector3 Dimensions { get; private set; }

    // TODO Find a more elegant way of making these static.
	[Export]
	public StringName[] WaypointTypeNames { get; set; } = Array.Empty<StringName>();

	[Export]
	public PackedScene[] WaypointTypeScenes { get; set; } = Array.Empty<PackedScene>();

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
				for (int i = 0; i < WaypointTypeNames.Length; i++)
				{
					waypointTypes.Add(WaypointTypeNames[i], WaypointTypeScenes[i]);
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
		#if DEBUG
		Name = Waypoint.Symbol;
		#endif

#region Create Waypoint
		if (OrbitalTarget != null) Scale = new Vector3(0.5f, 0.5f, 0.5f);

		Node3D waypointInstance;
		if (waypointTypes!.TryGetValue(Waypoint.Type.ToString(), out var scene))
		{
			waypointInstance = scene.Instantiate<Node3D>();
		}
		else
		{
			waypointInstance = waypointTypes["UNKNOWN"].Instantiate<Node3D>();
		}

		var waypointVisualiser = (IWaypointVisualiser)waypointInstance;

		waypointVisualiser.Waypoint = Waypoint;
		waypointVisualiser.OrbitCentre = OrbitalTarget == null ? SolarSystemCenter : OrbitalTarget.GlobalPosition;

		CallDeferred("add_child", waypointInstance);

		Dimensions = waypointVisualiser.Dimensions * Scale;
#endregion

#region Create Bounds
		bounds.Shape = new BoxShape3D()
        {
            Size = Dimensions / Scale
        };
#endregion

#region Position (And scale if in orbit)
		if (OrbitalTarget != null)
		{
			var rotation = 360 * OrbitalTarget.Waypoint.Orbitals.Select(x => x.Symbol).ToList().IndexOf(Waypoint.Symbol) / (OrbitalTarget.Waypoint.Orbitals.Count < 3 ? 3 : OrbitalTarget.Waypoint.Orbitals.Count);
			
			var startingPosition = new Vector3(OrbitalTarget.Dimensions.X, 0, OrbitalTarget.Dimensions.Z) / 2 + Dimensions / 2;

			Position += startingPosition.Rotated(new Vector3(0, 1, 0), Mathf.DegToRad(rotation));
		}
#endregion

#region Orbit Indicator
		if (OrbitalTarget == null)
		{
			orbitIndicator.GlobalPosition = SolarSystemCenter;
			orbitIndicator.GlobalRotation = new Vector3(0, Mathf.Atan2(GlobalPosition.X - SolarSystemCenter.X, GlobalPosition.Z - SolarSystemCenter.Z), 0);

			orbitIndicator.OrbitRadius = SolarSystemCenter.DistanceTo(new Vector3(GlobalPosition.X, 0, GlobalPosition.Z));
		}
		else
		{
			orbitIndicator.GlobalPosition = OrbitalTarget.GlobalPosition;
			orbitIndicator.GlobalRotation = new Vector3(0, Mathf.Atan2(GlobalPosition.X - OrbitalTarget.GlobalPosition.X, GlobalPosition.Z - OrbitalTarget.GlobalPosition.Z), 0);

			orbitIndicator.OrbitRadius = new Vector3(OrbitalTarget.GlobalPosition.X, 0, OrbitalTarget.GlobalPosition.Z).DistanceTo(new Vector3(GlobalPosition.X, 0, GlobalPosition.Z));			
		}

		orbitIndicator.Scale /= Scale;
#endregion

#region Name
		name.Text = Waypoint.Symbol;
		name.Position = new Vector3(0, new Vector2(Dimensions.Z / 2, Dimensions.Y / 2).Length(), 0) / Scale;
		name.Scale /= Scale;
#endregion

#region Indicators
		indicators.Position = new Vector3(0, -new Vector2(Dimensions.Z / 2, Dimensions.Y / 2).Length(), 0) / Scale;
		indicators.Scale /= Scale;

		SetIndicators(
			ships: false,
			market: Waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.MARKETPLACE),
			shipyard: Waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.SHIPYARD)
		);

		UINode.Instance.UIAppear += HideIndicators;
		UINode.Instance.UIDisappear += ShowIndicators;

		Waypoint.HasShips().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to fetch ships. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
				return;
			}

			SetIndicators(
				ships: t.Result,
				market: Waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.MARKETPLACE),
				shipyard: Waypoint.Traits.Select(t => t.Symbol).Contains(WaypointTraitSymbol.SHIPYARD)
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
					GameCameraNode.Instance.ZoomTo(GlobalPosition, Dimensions);
					UINode.Instance.UIDisappear += EndUIZoom;

					WaypointInfoNode.Show(Waypoint);

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

		OrbitalTarget?.ShowOrbit();
	}

	public void HideOrbit()
	{
		orbitIndicator.Visible = false;
		name.Visible = false;

		OrbitalTarget?.HideOrbit();
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
