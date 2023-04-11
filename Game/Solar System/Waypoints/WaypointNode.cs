using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosPeddler.Game.SolarSystem.Waypoints;

public partial class WaypointNode : Area3D
{
	private static Dictionary<string, PackedScene>? waypointTypes = null;

	public Waypoint Waypoint { get; set; } = null!;
	public Vector3 SolarSystemCenter { get; set; }
	public WaypointNode? OrbitalTarget { get; set; }

	public Vector3 Dimensions { get; private set; }

    // TODO Find a more elegant way of making these static.
	[Export]
	public string[] WaypointTypeNames { get; set; } = Array.Empty<string>();

	[Export]
	public PackedScene[] WaypointTypeScenes { get; set; } = Array.Empty<PackedScene>();

	private Label3D name = null!;
	private Node3D indicators = null!;
	private Sprite3D shipsIndicator = null!;
	private Sprite3D marketIndicator = null!;
	private Sprite3D shipyardIndicator = null!;

	public override void _EnterTree()
	{
#region Get Nodes
		name = GetNode<Label3D>("%Name");
		indicators = GetNode<Node3D>("%Indicators");
		shipsIndicator = GetNode<Sprite3D>("%Ships Indicator");
		marketIndicator = GetNode<Sprite3D>("%Market Indicator");
		shipyardIndicator = GetNode<Sprite3D>("%Shipyard Indicator");
#endregion
	}

    public override void _Ready()
	{
#region Generate Waypoint Types
        waypointTypes ??= WaypointTypeNames.Zip(WaypointTypeScenes).ToDictionary(pair => pair.First, pair => pair.Second);
#endregion

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

		var waypointVisualiser = (WaypointVisualiser)waypointInstance;

		waypointVisualiser.Waypoint = Waypoint;
		waypointVisualiser.OrbitCentre = OrbitalTarget == null ? SolarSystemCenter : OrbitalTarget.GlobalPosition;

		AddChild(waypointInstance);

		Dimensions = waypointVisualiser.Shape.GetDebugMesh().GetAabb().Size * Scale;
#endregion

#region Position (And scale if in orbit)
		if (OrbitalTarget != null)
		{
			var rotation = 360 * OrbitalTarget.Waypoint.Orbitals.Select(x => x.Symbol).ToList().IndexOf(Waypoint.Symbol) / (OrbitalTarget.Waypoint.Orbitals.Count < 3 ? 3 : OrbitalTarget.Waypoint.Orbitals.Count);
			
			var startingPosition = new Vector3(OrbitalTarget.Dimensions.X, 0, OrbitalTarget.Dimensions.Z) / 2 + Dimensions / 2;

			Position += startingPosition.Rotated(new Vector3(0, 1, 0), Mathf.DegToRad(rotation));
		}
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
}
