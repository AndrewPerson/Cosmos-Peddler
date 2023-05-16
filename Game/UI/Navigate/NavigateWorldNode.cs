using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CosmosPeddler.Game.SolarSystem;
using CosmosPeddler.Game.SolarSystem.Waypoints;

namespace CosmosPeddler.Game.UI.Navigate;

public partial class NavigateWorldNode : Node3D
{
	[Export]
    public PackedScene SolarSystemScene { get; set; } = null!;

    [Export]
    public PackedScene PathSegmentScene { get; set; } = null!;

    [Export]
	public float MapScale { get; set; } = 1.0f;

	[Export]
	public float WaypointMapScale { get; set; } = 1.0f;

    public bool CreatedWorld { get; private set; } = false;

    public string StartWaypointSymbol
    {
        get => startWaypointSymbol;
        set
        {
            startWaypointSymbol = value;
            UpdatePathUI();
        }
    }

    private string startWaypointSymbol = null!;

    public List<Stop> Path
    {
        get => path;
        set
        {
            path = value;
            UpdatePathUI();
        }
    }

    private Dictionary<string, WaypointNode> waypoints = new();

    private List<Stop> path = new();

    private GameCameraNode camera = null!;
    private Node systemsContainer = null!;
    private Node pathSegmentsPool = null!;

    private WaypointNode? hoveredWaypoint = null;

    public override void _EnterTree()
    {
        camera = GetNode<GameCameraNode>("%Camera");
        systemsContainer = GetNode("%Systems");
        pathSegmentsPool = GetNode("Path Segments Pool");
    }

    public async Task CreateWorld(Vector2 startingCameraMapPosition, SolarSystemNode.WaypointCreatedEventHandler waypointCreated)
    {
        if (CreatedWorld) return;
        CreatedWorld = true;

        camera.MapPosition = startingCameraMapPosition * MapScale;

        var systems = await CosmosPeddler.SolarSystem.GetSystems();

        foreach (var system in systems)
        {
            if (system == null) continue;

            var enabler = new VisibleOnScreenNotifier3D()
            {
                Aabb = SolarSystemNode.SystemExtents(system, WaypointMapScale),
                Position = new Vector3(system.X, 0, system.Y) * MapScale
            };

            enabler.ScreenEntered += InstantiateSystem;

            void InstantiateSystem()
            {
                enabler.ScreenEntered -= InstantiateSystem;

                ThreadPool.QueueUserWorkItem(_ => {
                    var systemInstance = SolarSystemScene.Instantiate<SolarSystemNode>();

                    systemInstance.System = system;
                    systemInstance.MapScale = MapScale;
                    systemInstance.WaypointMapScale = WaypointMapScale;
                    
                    systemInstance.WaypointCreated += waypointCreated;
                    systemInstance.WaypointCreated += waypoint => waypoints[waypoint.Waypoint.Symbol] = waypoint;

                    ThreadSafe.Run(systemInstance => enabler.AddChild(systemInstance), systemInstance);
                });
            }

            ThreadSafe.Run(enabler => AddChild(enabler), enabler);
        }
    }

    public void UpdatePathUI()
    {
        for (int i = 0; i < Path.Count; i++)
        {
            var segment = pathSegmentsPool.GetChildOrNull<PathSegmentNode>(i);

            if (segment == null)
            {
                segment = PathSegmentScene.Instantiate<PathSegmentNode>();
                pathSegmentsPool.AddChild(segment);
            }

            segment.Visible = true;

            var startSymbol = i == 0 ? StartWaypointSymbol : Path[i - 1].Symbol;
            var endSymbol = Path[i].Symbol;

            var startWaypoint = waypoints[startSymbol];
            var endWaypoint = waypoints[endSymbol];

            segment.Start = startWaypoint.GlobalPosition;
            segment.End = endWaypoint.GlobalPosition;
        }
    }
}
