using Godot;

namespace CosmosPeddler.Game.UI.Navigate;

public partial class PathSegmentNode : Node3D
{
	public Vector3 Start
    {
        get => start;
        set
        {
            start = value;
            Update();
        }
    }

    private Vector3 start;

    public Vector3 End
    {
        get => _end;
        set
        {
            _end = value;
            Update();
        }
    }

    private Vector3 _end;

    public Color Colour
    {
        get => colour;
        set
        {
            colour = value;
            segment.SetInstanceShaderParameter("colour", colour);
        }
    }

    private Color colour;

    public float Length => Start.DistanceTo(End);
    public Vector3 Direction => (End - Start).Normalized();

    private MeshInstance3D segment = null!;

    public override void _EnterTree()
    {
        segment = GetNode<MeshInstance3D>("%Segment");
    }

    private void Update()
    {
        Position = Start;
        Scale = new Vector3(Length, 1, Length);
        Rotation = new Vector3(0, Direction.AngleTo(new Vector3(0, 0, 1)), 0);

        segment.SetInstanceShaderParameter("dash_count", Mathf.Round(10 * Length));
        segment.SetInstanceShaderParameter("line_width", 0.05f / Length);
        segment.SetInstanceShaderParameter("time_scale", 10 / Length);
    }
}
