using Godot;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class WorldNode : Node3D
{
	[Export]
	public PackedScene solarSystemScene = null!;

	[Export]
	public float mapScale = 1.0f;

	[Export]
	public float waypointMapScale = 1.0f;

	private Node systemsContainer = null!;

	public override void _EnterTree()
	{
		systemsContainer = GetNode("%Systems");
	}

	public override void _Ready()
	{
		SpaceTradersClient.Load(OS.GetUserDataDir()).ContinueWith(async task =>
		{
			bool centredCamera = false;

			await foreach (var system in SpaceTradersClient.GetSystems(100))
			{
				if (!centredCamera)
				{
					GameCameraNode.Instance.SetDeferred("mapPosition", new Vector2(system.X, system.Y) * mapScale);
					centredCamera = true;
				}
				
				var systemInstance = solarSystemScene.Instantiate<SolarSystemNode>();

				#if DEBUG
				systemInstance.Name = system.Symbol;
				#endif

				systemInstance.system = system;
				systemInstance.mapScale = mapScale;
				systemInstance.waypointMapScale = waypointMapScale;

				systemsContainer.AddChild(systemInstance);
			}
		},
		TaskScheduler.FromCurrentSynchronizationContext());
	}
}
