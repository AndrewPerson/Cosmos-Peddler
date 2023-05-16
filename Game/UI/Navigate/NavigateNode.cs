using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosPeddler.Game.SolarSystem.Waypoints;

namespace CosmosPeddler.Game.UI.Navigate;

public partial class NavigateNode : PopupUI<Ship, NavigateNode>
{
    private WaypointNode? hoveredWaypoint = null;

    private WaypointNode? start = null;
    private WaypointNode? end = null;

    private bool generatingStops = false;
    private List<Stop> path = new();

    private NavigateWorldNode world = null!;

    public override void _EnterTree()
    {
        world = GetNode<NavigateWorldNode>("%World");
    }

    public override void UpdateUI()
    {
        world.StartWaypointSymbol = Data.Nav.WaypointSymbol;
    }

    public override void Shown()
    {
        if (world.CreatedWorld) return;

        Task.Run(async () =>
        {
            var system = await CosmosPeddler.SolarSystem.GetSystem(Data.Nav.SystemSymbol);

            await world.CreateWorld(new Vector2(system.X, system.Y), waypoint =>
            {
                waypoint.MouseEntered += () => {
                    hoveredWaypoint = waypoint;
                    GD.Print(waypoint.Name);
                };

                waypoint.MouseExited += () => {
                    if (hoveredWaypoint == waypoint) hoveredWaypoint = null;
                };
            });
        });
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                GD.Print(hoveredWaypoint?.Name);
                if (hoveredWaypoint != null && !generatingStops)
                {
                    if (start == null)
                    {
                        start = hoveredWaypoint;
                    }
                    else if (end == null)
                    {
                        end = hoveredWaypoint;

                        generatingStops = true;
                        GenerateStops(start.Waypoint, end.Waypoint, Data).ContinueWith(stops =>
                        {
                            start = null;
                            end = null;

                            generatingStops = false;

                            path.AddRange(stops.Result);
                            UpdatePathUI();
                        });
                    }
                }
            }
        }
    }

    private void UpdatePathUI()
    {
        
    }

    public async Task<List<Stop>> GenerateStops(Waypoint start, Waypoint end, Ship ship)
    {
        var stops = new List<Stop>();
        if (start.SystemSymbol == end.SystemSymbol)
        {
            stops.Add(new Stop(
                ship.HasWarpModule ?
                    new List<NavMode> { NavMode.Fly, NavMode.Warp } :
                    new List<NavMode> { NavMode.Fly },
                end.Symbol,
                end.SystemSymbol
            ));
        }
        else if (ship.HasWarpModule)
        {
            stops.Add(new Stop(
                new List<NavMode> { NavMode.Warp },
                end.Symbol,
                end.SystemSymbol
            ));
        }
        else
        {
            var startSystem = await CosmosPeddler.SolarSystem.GetSystem(start.SystemSymbol);
            var endSystem = await CosmosPeddler.SolarSystem.GetSystem(end.SystemSymbol);

            var startJumpGate = startSystem.Waypoints.First(w => w.Type == WaypointType.JUMP_GATE);
            var endJumpGate = endSystem.Waypoints.First(w => w.Type == WaypointType.JUMP_GATE);

            if (startJumpGate != null && endJumpGate != null)
            {
                stops.Add(new Stop(
                    new List<NavMode> { NavMode.Fly },
                    startJumpGate.Symbol,
                    startSystem.Symbol
                ));

                stops.Add(new Stop(
                    new List<NavMode> { NavMode.Jump },
                    endJumpGate.Symbol,
                    endSystem.Symbol
                ));

                stops.Add(new Stop(
                    new List<NavMode> { NavMode.Fly },
                    end.Symbol,
                    end.SystemSymbol
                ));
            }
        }

        return stops;
    }
}
