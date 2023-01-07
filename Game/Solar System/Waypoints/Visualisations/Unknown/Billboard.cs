using Godot;

namespace CosmosPeddler.Game;

public partial class Billboard : Node3D
{
    private Node3D camera = null!;

    public override void _Ready()
    {
        camera = GetTree().CurrentScene.GetNode<Camera3D>("%Camera");
    }

    public override void _Process(double delta)
    {
        LookAt(GlobalPosition + new Quaternion(camera.Basis) * Vector3.Forward, new Quaternion(camera.Basis) * Vector3.Up);
    }
}