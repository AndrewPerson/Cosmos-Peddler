using Godot;
using System.Threading.Tasks;

namespace CosmosPeddler.Home;

public partial class LoginNode : Control
{
	private AdvancedLineEdit tokenInput = null!;

	public override void _EnterTree()
	{
		tokenInput = GetNode<AdvancedLineEdit>("%Token Input");
	}

	private async Task<string?> ValidateToken(string token)
	{
		if (await SpaceTradersClient.ValidToken(token))
		{
			return null;
		}
		else
		{
			return "Invalid token";
		}
	}

	public void Login()
	{
		tokenInput.Validate(ValidateToken).ContinueWith(async task =>
		{
			if (task.Result)
			{
				await SpaceTradersClient.Login(tokenInput.Text);
				await SpaceTradersClient.Save(OS.GetUserDataDir());
				GetTree().ChangeSceneToFile("res://Game/Game.tscn");
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
