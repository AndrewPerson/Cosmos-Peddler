using Godot;

[Tool]
public partial class AutosizeSubviewport : SubViewport
{
	public override void _Process(double delta)
	{
        if (GetParent() is Control parent)
        {
            Size = (Vector2I)parent.Size;
        }
    }
}
