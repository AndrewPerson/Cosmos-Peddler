using Godot;
using CosmosPeddler.UI;

namespace CosmosPeddler.Game.SolarSystem.Waypoints.Visualisations;

public partial class UnknownNode : Billboard, IWaypointVisualiser
{
	public Vector3 Dimensions { get; private set; } = new Vector3(0.5f, 1, 0.25f);

	public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;
}
