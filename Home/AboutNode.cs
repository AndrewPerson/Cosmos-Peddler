using Godot;

namespace CosmosPeddler.Home;

using Engine = Godot.Engine;

public partial class AboutNode : Control
{
	private RichTextLabel license = null!;

	public override void _EnterTree()
	{
		license = GetNode<RichTextLabel>("%License");
	}

    public override void _Ready()
    {
        license.PushParagraph(HorizontalAlignment.Center);
		license.PushOutlineSize(3);
		license.PushOutlineColor(Color.Color8(0, 0, 0));

		license.AddText(Engine.GetLicenseText());

		license.Pop();
		license.Pop();
		license.Pop();
    }

	public void OpenLink(string link)
	{
		OS.ShellOpen(link);
	}
}
