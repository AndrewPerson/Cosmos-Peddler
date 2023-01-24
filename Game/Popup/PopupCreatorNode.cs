using Godot;

namespace CosmosPeddler.Game;

public partial class PopupCreatorNode : Node
{
	private static PopupCreatorNode instance = null!;

	[Export]
	public PackedScene popup = null!;

	public static void CreatePopup(PopupType type, string text)
	{
		var popup = instance.popup.Instantiate<PopupNode>();
		popup.type = type;
		popup.text = text;
		instance.AddChild(popup);
	}

	public override void _Ready()
	{
		instance = this;
	}
}
