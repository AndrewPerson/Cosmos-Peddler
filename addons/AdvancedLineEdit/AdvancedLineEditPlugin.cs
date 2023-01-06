#if TOOLS
using Godot;

[Tool]
public partial class AdvancedLineEditPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		var script = GD.Load<Script>("addons/AdvancedLineEdit/AdvancedLineEdit.cs");
        var texture = GD.Load<Texture2D>("addons/AdvancedLineEdit/icon.svg");
        AddCustomType("AdvancedLineEdit", "LineEdit", script, texture);
	}

	public override void _ExitTree()
	{
		RemoveCustomType("AdvancedLineEdit");
	}
}
#endif
