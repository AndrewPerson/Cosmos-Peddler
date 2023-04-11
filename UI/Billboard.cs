using Godot;

namespace CosmosPeddler.UI;

public partial class Billboard : Node3D
{
    private Node3D camera = null!;

    public override void _Ready()
    {
        camera = GetViewport().GetCamera3D();
    }

    public override void _Process(double delta)
    {
        LookAt(GlobalPosition + new Quaternion(camera.Basis) * Vector3.Forward, new Quaternion(camera.Basis) * Vector3.Up);
    }
}