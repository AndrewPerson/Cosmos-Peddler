using Godot;

public partial class UINode : Control
{
	private Control home = null!;
	private Control login = null!;
	private Control register = null!;
	private Control homeButton = null!;

	public override void _EnterTree()
	{
		home = GetNode<Control>("%Home");
		login = GetNode<Control>("%Login");
		register = GetNode<Control>("%Register");
		homeButton = GetNode<Control>("%Home Button");
	}

	public void ShowHome()
	{
		home.Visible = true;
		login.Visible = false;
		register.Visible = false;
		homeButton.Visible = false;
	}

	public void ShowLogin()
	{
		home.Visible = false;
		login.Visible = true;
		register.Visible = false;
		homeButton.Visible = true;
	}

	public void ShowRegister()
	{
		home.Visible = false;
		login.Visible = false;
		register.Visible = true;
		homeButton.Visible = true;
	}
}
