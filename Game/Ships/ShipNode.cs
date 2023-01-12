using Godot;

namespace CosmosPeddler.Game.Ships;

//TODO Make this be procedurally generated
public partial class ShipNode : Node3D
{
    public Ship ship = null!;

    public Vector3 frame;
    public Vector3 reactor;
    public Vector3 engine;
    public Vector3 modules;
    public Vector3 mounts;
}