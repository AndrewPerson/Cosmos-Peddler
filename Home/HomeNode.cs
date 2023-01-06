using System.Threading.Tasks;
using Godot;

namespace CosmosPeddler.Home;

public partial class HomeNode : Node
{
	public override void _Ready()
	{
		SpaceTradersClient.Load(OS.GetUserDataDir()).ContinueWith(t =>
		{
			if (t.Result)
			{
				GetTree().ChangeSceneToFile("res://Game/Game.tscn");
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
