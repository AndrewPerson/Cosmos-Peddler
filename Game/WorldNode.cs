using Godot;
using System.Threading;
using System.Collections.Generic;
using CosmosPeddler.Game.UI;
using CosmosPeddler.Game.UI.WaypointInfo;
using CosmosPeddler.Game.SolarSystem;
using CosmosPeddler.Game.SolarSystem.Waypoints;
using CosmosPeddler.Game.SolarSystem.OrbitIndicator;

namespace CosmosPeddler.Game;

public partial class WorldNode : Node3D
{
	[Export]
	public PackedScene SolarSystemScene { get; set; } = null!;

    [Export]
    public PackedScene OrbitIndicatorScene { get; set; } = null!;

	[Export]
	public float MapScale { get; set; } = 1.0f;

	[Export]
	public float WaypointMapScale { get; set; } = 1.0f;

    private Dictionary<SolarSystemNode, List<WaypointNode>> systemWaypoints = new();

    private GameCameraNode camera = null!;
	private Node systemsContainer = null!;
    private Node orbitIndicatorPool = null!;

    private Stack<OrbitIndicatorNode> unusedOrbitIndicators = new();
    private Dictionary<WaypointNode, OrbitIndicatorNode> usedOrbitIndicators = new();

    private WaypointNode? hoveredWaypoint = null;

	public override void _EnterTree()
	{
        camera = GetNode<GameCameraNode>("%Camera");
		systemsContainer = GetNode("%Systems");
        orbitIndicatorPool = GetNode("%Orbit Indicator Pool");
	}

	public override void _Ready()
	{
		SpaceTradersClient.Load(OS.GetUserDataDir()).ContinueWith(async task =>
		{
            bool centredCamera = false;
            var systems = await CosmosPeddler.SolarSystem.GetSystems();

			foreach (var system in systems)
			{
                if (system == null) continue;

                if (!centredCamera)
                {
                    camera.MapPosition = new Vector2(system.X, system.Y) * MapScale;
                    centredCamera = true;
                }

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
                        
                        systemInstance.WaypointCreated += waypoint => {
                            var waypoints = systemWaypoints.GetValueOrDefault(systemInstance, new List<WaypointNode>());
                            waypoints.Add(waypoint);
                            systemWaypoints[systemInstance] = waypoints;

                            if (waypoint.OrbitalTarget == null)
                            {
                                waypoint.MouseEntered += () => ShowIndicator(waypoint, systemInstance.GlobalPosition);
                            }
                            else
                            {
                                waypoint.MouseEntered += () => ShowIndicator(waypoint, waypoint.OrbitalTarget.GlobalPosition);
                            }

                            waypoint.MouseEntered += () => hoveredWaypoint = waypoint;
                            
                            waypoint.MouseExited += () => HideIndicator(waypoint);
                            waypoint.MouseExited += () => {
                                if (hoveredWaypoint == waypoint)
                                {
                                    hoveredWaypoint = null;
                                }
                            };

                            UINode.Instance.UIAppear += waypoint.HideIndicators;
                            UINode.Instance.UIDisappear += waypoint.ShowIndicators;

                            waypoint.TreeExited += () => {
                                UINode.Instance.UIAppear -= waypoint.HideIndicators;
                                UINode.Instance.UIDisappear -= waypoint.ShowIndicators;
                            };
                        };

                        systemInstance.MouseEntered += () => ShowAllIndicators(systemInstance);
                        systemInstance.MouseExited += () => HideAllIndicators(systemInstance);

                        ThreadSafe.Run(systemInstance => enabler.AddChild(systemInstance), systemInstance);
                    });
                }

                ThreadSafe.Run(enabler => AddChild(enabler), enabler);
			}
		});
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                if (hoveredWaypoint != null)
                {
                    camera.ZoomTo(hoveredWaypoint.GlobalPosition, hoveredWaypoint.Dimensions);
                    UINode.Instance.UIDisappear += EndUIZoom;

                    WaypointInfoNode.Show(hoveredWaypoint.Waypoint);
                }
            }
        }
    }

    private void EndUIZoom()
    {
        UINode.Instance.UIDisappear -= EndUIZoom;
        camera.EndZoom();
    }

    private void ShowIndicator(WaypointNode waypoint, Vector3 orbitCentre)
    {
        if (usedOrbitIndicators.TryGetValue(waypoint, out var indicator))
        {
            indicator.Visible = true;
            return;
        }

        if (unusedOrbitIndicators.Count == 0)
        {
            indicator = OrbitIndicatorScene.Instantiate<OrbitIndicatorNode>();
            orbitIndicatorPool.AddChild(indicator);
        }
        else
        {
            indicator = unusedOrbitIndicators.Pop();
        }

        var flatWaypointPosition = new Vector3(waypoint.GlobalPosition.X, 0, waypoint.GlobalPosition.Z);
        var flatOrbitCentre = new Vector3(orbitCentre.X, 0, orbitCentre.Z);

        indicator.GlobalPosition = flatOrbitCentre;
        indicator.LookAt(flatWaypointPosition, Vector3.Up);
        indicator.RotateY(Mathf.Pi);

        indicator.OrbitRadius = flatWaypointPosition.DistanceTo(flatOrbitCentre);

        indicator.Visible = true;

        usedOrbitIndicators.Add(waypoint, indicator);

        // TODO Show orbit indicator for a waypoint's orbital target if it has one.
    }

    private void HideIndicator(WaypointNode waypoint)
    {
        if (usedOrbitIndicators.TryGetValue(waypoint, out var indicator))
        {
            indicator.Visible = false;
            unusedOrbitIndicators.Push(indicator);
            usedOrbitIndicators.Remove(waypoint);
        }
    }

    private void ShowAllIndicators(SolarSystemNode system)
    {
        if (systemWaypoints.TryGetValue(system, out var waypoints))
        {
            foreach (var waypoint in waypoints)
            {
                if (waypoint.OrbitalTarget == null)
                {
                    ShowIndicator(waypoint, system.GlobalPosition);
                }
                else
                {
                    ShowIndicator(waypoint, waypoint.OrbitalTarget.GlobalPosition);
                }
            }
        }
    }

    private void HideAllIndicators(SolarSystemNode system)
    {
        if (systemWaypoints.TryGetValue(system, out var waypoints))
        {
            foreach (var waypoint in waypoints)
            {
                HideIndicator(waypoint);
            }
        }
    }
}
