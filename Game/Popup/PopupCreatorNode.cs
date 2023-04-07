using Godot;

namespace CosmosPeddler.Game;

public partial class PopupCreatorNode : Node
{
	private static PopupCreatorNode instance = null!;

	[Export]
	public PackedScene Popup { get; set; } = null!;

    public static void CreatePopupThreadSafe(PopupType type, string text)
    {
        ThreadSafe.Run(d => CreatePopup(d.type, d.text), (type, text));
    }

	public static void CreatePopup(PopupType type, string text)
	{
		var popup = instance.Popup.Instantiate<PopupNode>();
		popup.Type = type;
		popup.Text = text;
		instance.AddChild(popup);
	}

	public override void _Ready()
	{
		instance = this;
	}
}
