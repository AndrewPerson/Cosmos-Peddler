using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosPeddler.Game.SolarSystem.Stars;
using CosmosPeddler.Game.SolarSystem.Waypoints;

namespace CosmosPeddler.Game.SolarSystem;

public partial class SolarSystemNode : Node3D
{
	[Export]
	public PackedScene starScene = null!;

	[Export]
	public PackedScene waypointScene = null!;

	public CosmosPeddler.SolarSystem system = null!;

	public float mapScale = 1.0f;

	public float waypointMapScale = 1.0f;

	private StarNode star = null!;
	private Node waypointContainer = null!;
	private VisibleOnScreenNotifier3D visibilityNotifier = null!;

	public override void _EnterTree()
	{
		waypointContainer = GetNode("%Waypoints");
		visibilityNotifier = GetNode<VisibleOnScreenNotifier3D>("%Visibility Notifier");
	}

	public override void _Ready()
	{
		Position = new Vector3(system.X, 0, system.Y) * mapScale;

		star = starScene.Instantiate<StarNode>();

		star.systemType = system.Type;
		star.MouseEnter += ShowOrbits;
		star.MouseExit += HideOrbits;

		AddChild(star);

		var maxX = system.Waypoints.Count == 0 ? 1 : system.Waypoints.Max(waypoint => waypoint.X) * waypointMapScale + 10;
		var minX = system.Waypoints.Count == 0 ? -1 : system.Waypoints.Min(waypoint => waypoint.X) * waypointMapScale - 10;
		var maxY = system.Waypoints.Count == 0 ? 1 : system.Waypoints.Max(waypoint => waypoint.Y) * waypointMapScale + 10;
		var minY = system.Waypoints.Count == 0 ? -1 : system.Waypoints.Min(waypoint => waypoint.Y) * waypointMapScale - 10;

		var max = new Vector2(maxX, maxY);
		var min = new Vector2(minX, minY);

		var centre = (min - max) / 2;
		var size = max - min;

		visibilityNotifier.Aabb = new AABB(new Vector3(centre.x, 0, centre.y), new Vector3(size.x, .5f, size.y));
	}

	public override void _ExitTree()
	{
		if (star == null) return;

		star.MouseEnter -= ShowOrbits;
		star.MouseExit -= HideOrbits;
	}

	public void InstantiateWaypoints()
	{
		InstantiateWaypointsAsync().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				GD.PrintErr(t.Exception);
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}

	private async Task InstantiateWaypointsAsync()
	{
		var waypoints = new Dictionary<string, Waypoint>();
		await foreach (var waypoint in system.GetWaypoints())
		{
			waypoints.Add(waypoint.Symbol, waypoint);
		}

		var orbitalWaypoints = new Dictionary<Waypoint, Waypoint>();

		foreach (var (_, waypoint) in waypoints)
		{
			foreach (var orbital in waypoint.Orbitals)
			{
				orbitalWaypoints.Add(waypoints[orbital.Symbol], waypoint);
			}
		}

		var nonOrbitalWaypoints = waypoints.Where(waypoint => orbitalWaypoints.ContainsKey(waypoint.Value) == false);

		var waypointNodeDict = new Dictionary<Waypoint, WaypointNode>();

		foreach (var (_, waypoint) in nonOrbitalWaypoints)
		{
			var waypointInstance = waypointScene.Instantiate<WaypointNode>();

			waypointInstance.waypoint = waypoint;
			waypointInstance.solarSystemCenter = new Vector3(system.X, 0, system.Y) * mapScale;
			waypointInstance.mapScale = waypointMapScale;

			waypointContainer.AddChild(waypointInstance);

			waypointNodeDict.Add(waypoint, waypointInstance);
		}

		foreach (var (waypoint, orbitalTarget) in orbitalWaypoints)
		{
			var waypointInstance = waypointScene.Instantiate<WaypointNode>();

			waypointInstance.waypoint = waypoint;
			waypointInstance.solarSystemCenter = new Vector3(system.X, 0, system.Y) * mapScale;
			waypointInstance.mapScale = waypointMapScale;

			waypointInstance.orbitalTarget = waypointNodeDict[orbitalTarget];

			waypointContainer.AddChild(waypointInstance);
		}
	}

	public void ShowOrbits()
	{
		foreach (var waypointNode in waypointContainer.GetChildren())
		{
			((WaypointNode)waypointNode).ShowOrbit();
		}
	}

	public void HideOrbits()
	{
		foreach (var waypointNode in waypointContainer.GetChildren())
		{
			((WaypointNode)waypointNode).HideOrbit();
		}
	}
}
