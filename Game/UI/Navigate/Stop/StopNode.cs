using Godot;
using System.Collections.Generic;
using CosmosPeddler.Game.SolarSystem.Waypoints;

namespace CosmosPeddler.Game.UI.Navigate;

public record StopNodeData(IList<Stop> Stops, int SelectedStopIndex);
public record Stop(IList<NavMode> NavOptions, string Symbol, string SystemSymbol);

public partial class StopNode : ReactiveUI<StopNodeData>
{
    [Signal]
    public delegate void SelectionChangedEventHandler(int stopIndex, int navOptionIndex);

	private OptionButton to = null!;
    private OptionButton method = null!;
    private Node3D waypointContainer = null!;
    private Camera3D waypointCamera = null!;

    public override void _EnterTree()
    {
        to = GetNode<OptionButton>("%To");
        method = GetNode<OptionButton>("%Method");
        waypointContainer = GetNode<Node3D>("%Waypoint Container");
        waypointCamera = GetNode<Camera3D>("%Waypoint Camera");
    }

    public override void _Ready()
    {
        to.ItemSelected += UpdateWaypoint;
        to.ItemSelected += index => EmitSignal(SignalName.SelectionChanged, (int)index, method.Selected);

        method.ItemSelected += index => EmitSignal(SignalName.SelectionChanged, to.Selected, (int)index);
    }

    public override void UpdateUI()
    {
        to.Clear();
        method.Clear();

        foreach (var stop in Data.Stops)
        {
            to.AddItem(stop.Symbol);
        }

        to.Select(Data.SelectedStopIndex);

        foreach (var navOption in Data.Stops[Data.SelectedStopIndex].NavOptions)
        {
            method.AddItem(navOption.ToString());
        }

        UpdateWaypoint(Data.SelectedStopIndex);
    }

    private void UpdateWaypoint(long index)
    {
        var stop = Data.Stops[(int)index];

        waypointContainer.RemoveChild(waypointContainer.GetChild(0));
        Waypoint.GetWaypoint(stop.Symbol, stop.SystemSymbol).ContinueWith(t =>
        {
            var waypoint = new WaypointNode();
            waypoint.Waypoint = t.Result;
            waypoint.SolarSystemCenter = Vector3.Zero;
            waypoint.OrbitalTarget = null;

            ThreadSafe.Run(waypoint => waypointContainer.AddChild(waypoint), waypoint);

            var dimensions = waypoint.Dimensions;

            var size = Mathf.Sqrt(dimensions.X * dimensions.X + dimensions.Y * dimensions.Y + dimensions.Z * dimensions.Z);

            var distance = size / 1.5f / Mathf.Tan(Mathf.DegToRad(waypointCamera.Fov / 2));

            waypointCamera.Position = new Vector3(0, 0, distance);
        });
    }
}
