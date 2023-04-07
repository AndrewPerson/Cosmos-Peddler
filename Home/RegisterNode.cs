using Godot;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CosmosPeddler.Home;

public partial class RegisterNode : Control
{
	private AdvancedLineEdit nameInput = null!;
	private OptionButton factionInput = null!;

	public override void _EnterTree()
	{
		nameInput = GetNode<AdvancedLineEdit>("%Name Input");
		factionInput = GetNode<OptionButton>("%Faction Input");
	}

	public void Register()
	{
		string token = "";
		async Task<string?> ValidateRegistrationDetails(string name)
		{
			try
			{
				var faction = factionInput.Selected;

				token = await SpaceTradersClient.Register(Enum.GetValues<BodyFaction>()[faction], name);

				return null;
			}
			catch (ApiException e)
			{
				try
				{
					var json = JsonDocument.Parse(e.Response);
					
					var error = json.RootElement.GetProperty("error").GetProperty("data");
					
					if (error.TryGetProperty("symbol", out var symbol))
					{
						return symbol.EnumerateArray().Aggregate("", (acc, cur) => acc + cur.GetString() + " ").Trim();
					}
					else
					{
						return error.ToString();
					}
				}
				catch (Exception)
				{
					return e.InnerException?.Message ?? e.Message;
				}
			}
			catch (Exception e)
			{
				return e.InnerException?.Message ?? e.Message;
			}
		}

		nameInput.Validate(ValidateRegistrationDetails).ContinueWith(async task =>
		{
			if (task.Result)
			{
				await SpaceTradersClient.Login(token);
				await SpaceTradersClient.Save(OS.GetUserDataDir());
				ThreadSafe.Run(() => GetTree().ChangeSceneToFile("res://Game/Game.tscn"));
			}
		});
	}
}
