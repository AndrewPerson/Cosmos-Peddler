using Godot;
using System.Collections.Generic;

namespace CosmosPeddler.Home;

using Engine = Godot.Engine;

public partial class AboutNode : Control
{
	private RichTextLabel license = null!;

	private readonly Dictionary<string, string> licenses = new()
	{
		{ "Godot License", Engine.GetLicenseText() },
		{ "Spaceship Model License", "Spaceship Concept by Yogoshimo 2.0 [CC-BY] (https://creativecommons.org/licenses/by/3.0/) via Poly Pizza (https://poly.pizza/m/9pBgIlbtj3F)" }
	};

	public override void _EnterTree()
	{
		license = GetNode<RichTextLabel>("%License");
	}

    public override void _Ready()
    {
		license.PushOutlineSize(3);
		license.PushOutlineColor(Color.Color8(0, 0, 0));

		foreach (var (licenseName, licenseText) in licenses)
		{
			AddLicense(licenseName, licenseText);
		}

		foreach (var (licenseName, licenseText) in Engine.GetLicenseInfo())
		{
			AddLicense(licenseName.AsString(), licenseText.AsString());
		}

		license.Pop();
		license.Pop();
    }

	private void AddLicense(string licenseName, string licenseText)
	{
		license.PushBold();
		license.AddText($"{licenseName}:");
		license.Newline();

		license.AddText(licenseText);
		license.Newline();
		license.AddText(" ");
		license.Newline();
	}

	public void OpenLink(string link)
	{
		OS.ShellOpen(link);
	}
}
