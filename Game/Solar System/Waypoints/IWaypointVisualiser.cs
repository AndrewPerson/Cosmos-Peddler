using Godot;

namespace CosmosPeddler.Game;

public interface IWaypointVisualiser : IDimensionedObject
{
    public Vector3 SolarSystemCentre { get; set; }
    public Waypoint Waypoint { get; set; }
}