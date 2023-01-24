using Godot;
using System;

namespace CosmosPeddler.Game;

public partial class PopupNode : PanelContainer
{
	public PopupType type;
	public string text = null!;

	[Export]
	public Texture2D successTex = null!;
	[Export]
	public Color successCol;
	
	[Export]
	public Texture2D infoTex = null!;
	[Export]
	public Color infoCol;
	
	[Export]
	public Texture2D warningTex = null!;
	[Export]
	public Color warningCol;
	
	[Export]
	public Texture2D errorTex = null!;
	[Export]
	public Color errorCol;

	private ColorRect colour = null!;
	private TextureRect icon = null!;
	private Label textLabel = null!;

	public override void _Ready()
	{
		colour = GetNode<ColorRect>("%Colour");
		icon = GetNode<TextureRect>("%Icon");
		textLabel = GetNode<Label>("%Text");

		colour.Modulate = type switch
		{
			PopupType.Success => successCol,
			PopupType.Info => infoCol,
			PopupType.Warning => warningCol,
			PopupType.Error => errorCol,
			_ => throw new ArgumentOutOfRangeException()
		};

		icon.Texture = type switch
		{
			PopupType.Success => successTex,
			PopupType.Info => infoTex,
			PopupType.Warning => warningTex,
			PopupType.Error => errorTex,
			_ => throw new ArgumentOutOfRangeException()
		};

		textLabel.Text = text;
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
