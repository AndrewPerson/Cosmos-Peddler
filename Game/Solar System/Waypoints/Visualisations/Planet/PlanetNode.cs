using Godot;

namespace CosmosPeddler.Game;

public partial class PlanetNode : MeshInstance3D, IWaypointVisualiser
{
    public Vector3 Dimensions { get; } = new Vector3(0.5f, 0.5f, 0.5f);
    
    public Vector3 OrbitCentre { get; set; }
    public Waypoint Waypoint { get; set; } = null!;

    private MeshInstance3D ocean = null!;

    public override void _Ready()
    {
        base._Ready();

        ocean = GetNode<MeshInstance3D>("%Ocean");

        var seed = (float)GD.RandRange(0f, 10f);
        SetSeed(seed);

        GetNode<AnimationPlayer>("%Rotation").Play("Rotate");
    }

    private void SetSeed(float seed)
    {
        SetInstanceShaderParameter("seed", seed);
        ocean.SetInstanceShaderParameter("seed", seed);
    }
}
