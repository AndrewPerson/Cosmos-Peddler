using Godot;

namespace CosmosPeddler.Game.Ships;

//TODO Make this be procedurally generated
public partial class ShipNode : Node3D
{
    public Ship Ship { get; set; } = null!;

    public Vector3 FramePosition { get; private set; }
    public Vector3 ReactorPosition { get; private set; }
    public Vector3 EnginePosition { get; private set; }
    public Vector3 ModulesPosition { get; private set; }
    public Vector3 MountsPosition { get; private set; }
}