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
	public PackedScene StarScene { get; set; } = null!;

	[Export]
	public PackedScene WaypointScene { get; set; } = null!;

	public CosmosPeddler.SolarSystem system { get; set; } = null!;

	public float MapScale { get; set; }

	public float WaypointMapScale { get; set; }

	private StarNode star = null!;
	private Node waypointContainer = null!;

    public static Aabb SystemExtents(CosmosPeddler.SolarSystem system, float waypointScale)
    {
        var maxX = system.Waypoints.Count == 0 ? 1 : system.Waypoints.Max(waypoint => waypoint.X) * waypointScale;
		var minX = system.Waypoints.Count == 0 ? -1 : system.Waypoints.Min(waypoint => waypoint.X) * waypointScale;
		var maxY = system.Waypoints.Count == 0 ? 1 : system.Waypoints.Max(waypoint => waypoint.Y) * waypointScale;
		var minY = system.Waypoints.Count == 0 ? -1 : system.Waypoints.Min(waypoint => waypoint.Y) * waypointScale;

		var max = new Vector2(maxX, maxY);
		var min = new Vector2(minX, minY);

		var centre = (min - max) / 2;
		var size = max - min;

        return new Aabb(new Vector3(centre.X, 0, centre.Y), new Vector3(size.X, .5f, size.Y));
    }

	public override void _EnterTree()
	{
		waypointContainer = GetNode("%Waypoints");
	}

	public override void _Ready()
	{
		#if DEBUG
		Name = system.Symbol ?? "Unnamed System";
		#endif

		star = StarScene.Instantiate<StarNode>();

		star.SystemType = system.Type;
		star.MouseEntered += ShowOrbits;
		star.MouseExited += HideOrbits;

		AddChild(star);

        InstantiateWaypointsAsync().ContinueWith(t =>
		{
			if (t.IsFaulted)
			{
				PopupCreatorNode.CreatePopup(PopupType.Error, "Failed to instantiate waypoints. Check the log for errors.");
				Logger.Error(t.Exception?.ToString() ?? "Unknown error");
			}
		},
        TaskScheduler.FromCurrentSynchronizationContext());
	}

	public override void _ExitTree()
	{
		if (star == null) return;

		star.MouseEntered -= ShowOrbits;
		star.MouseExited -= HideOrbits;
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
			var waypointInstance = WaypointScene.Instantiate<WaypointNode>();

			waypointInstance.Waypoint = waypoint;
			waypointInstance.SolarSystemCenter = new Vector3(system.X, 0, system.Y) * MapScale;

            waypointInstance.Position = new Vector3(waypoint.X, 0, waypoint.Y) * WaypointMapScale;

			waypointContainer.AddChild(waypointInstance);

			waypointNodeDict.Add(waypoint, waypointInstance);
		}

		foreach (var (waypoint, orbitalTarget) in orbitalWaypoints)
		{
			var waypointInstance = WaypointScene.Instantiate<WaypointNode>();

			waypointInstance.Waypoint = waypoint;
			waypointInstance.SolarSystemCenter = new Vector3(system.X, 0, system.Y) * MapScale;

			waypointInstance.OrbitalTarget = waypointNodeDict[orbitalTarget];

            waypointInstance.Position = new Vector3(waypoint.X, 0, waypoint.Y) * WaypointMapScale;

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
