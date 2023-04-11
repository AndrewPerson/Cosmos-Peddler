using Godot;

namespace CosmosPeddler.Game.SolarSystem.Waypoints;

public abstract partial class WaypointVisualiser : CollisionShape3D
{
    public abstract Vector3 OrbitCentre { get; set; }
    public abstract Waypoint Waypoint { get; set; }
}