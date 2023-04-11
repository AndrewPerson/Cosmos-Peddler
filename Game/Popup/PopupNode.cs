using Godot;
using System;

namespace CosmosPeddler.Game;

public partial class PopupNode : PanelContainer
{
	public PopupType Type { get; set; }
	public string Text { get; set; } = "";

	[Export]
	public Texture2D SuccessTex { get; set; } = null!;
	[Export]
	public Color SuccessCol { get; set; }
	
	[Export]
	public Texture2D InfoTex { get; set; } = null!;
	[Export]
	public Color InfoCol { get; set; }
	
	[Export]
	public Texture2D WarningTex { get; set; } = null!;
	[Export]
	public Color WarningCol { get; set; }
	
	[Export]
	public Texture2D ErrorTex { get; set; } = null!;
	[Export]
	public Color ErrorCol { get; set; }

	private ColorRect colour = null!;
	private TextureRect icon = null!;
	private Label textLabel = null!;

    public override void _EnterTree()
    {
		colour = GetNode<ColorRect>("%Colour");
		icon = GetNode<TextureRect>("%Icon");
		textLabel = GetNode<Label>("%Text");
    }

	public override void _Ready()
	{
		colour.Modulate = Type switch
		{
			PopupType.Success => SuccessCol,
			PopupType.Info => InfoCol,
			PopupType.Warning => WarningCol,
			PopupType.Error => ErrorCol,
			_ => throw new ArgumentOutOfRangeException()
		};

		icon.Texture = Type switch
		{
			PopupType.Success => SuccessTex,
			PopupType.Info => InfoTex,
			PopupType.Warning => WarningTex,
			PopupType.Error => ErrorTex,
			_ => throw new ArgumentOutOfRangeException()
		};

		textLabel.Text = Text;
	}

	public void Close()
	{
		QueueFree();
	}
}

public enum PopupType
{
	Success,
	Info,
	Warning,
	Error
}
