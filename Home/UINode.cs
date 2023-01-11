using Godot;

namespace CosmosPeddler.Home;

public partial class UINode : Control
{
	private Control home = null!;
	private Control login = null!;
	private Control register = null!;
	private Control about = null!;
	private Control homeButton = null!;

	private Control prevScreen = null!;

	public override void _EnterTree()
	{
		home = GetNode<Control>("%Home");
		login = GetNode<Control>("%Login");
		register = GetNode<Control>("%Register");
		about = GetNode<Control>("%About");
		homeButton = GetNode<Control>("%Home Button");

		prevScreen = home;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			ShowHome();
		}
	}

	public void ShowHome()
	{
		if (prevScreen != null) prevScreen.Visible = false;
		home.Visible = true;
		prevScreen = home;

		homeButton.Visible = false;
	}

	public void ShowLogin()
	{
		if (prevScreen != null) prevScreen.Visible = false;
		login.Visible = true;
		prevScreen = login;

		homeButton.Visible = true;
	}

	public void ShowRegister()
	{
		if (prevScreen != null) prevScreen.Visible = false;
		register.Visible = true;
		prevScreen = register;

		homeButton.Visible = true;
	}

	public void ShowAbout()
	{
		if (prevScreen != null) prevScreen.Visible = false;
		about.Visible = true;
		prevScreen = about;

		homeButton.Visible = true;
	}
}
