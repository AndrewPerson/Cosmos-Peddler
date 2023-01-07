using Godot;

namespace CosmosPeddler.Game;

public interface IWaypointVisualiser : IDimensionedObject
{
    public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; }
}