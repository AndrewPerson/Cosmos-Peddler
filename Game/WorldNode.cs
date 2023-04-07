using Godot;
using System.Threading;
using CosmosPeddler.Game.SolarSystem;

namespace CosmosPeddler.Game;

public partial class WorldNode : Node3D
{
	[Export]
	public PackedScene SolarSystemScene { get; set; } = null!;

	[Export]
	public float MapScale { get; set; } = 1.0f;

	[Export]
	public float WaypointMapScale { get; set; } = 1.0f;

	private Node systemsContainer = null!;

	public override void _EnterTree()
	{
		systemsContainer = GetNode("%Systems");
	}

	public override void _Ready()
	{
        var synchronisationContext = GodotSynchronizationContext.Current!;
		SpaceTradersClient.Load(OS.GetUserDataDir()).ContinueWith(async task =>
		{
            bool centredCamera = false;
            var systems = await CosmosPeddler.SolarSystem.GetSystems();

			foreach (var system in systems)
			{
                if (system == null) continue;

                if (!centredCamera)
                {
                    GameCameraNode.Instance.MapPosition = new Vector2(system.X, system.Y) * MapScale;
                    centredCamera = true;
                }

                var enabler = new VisibleOnScreenNotifier3D()
                {
                    Aabb = SolarSystemNode.SystemExtents(system, WaypointMapScale),
                    Position = new Vector3(system.X, 0, system.Y) * MapScale
                };

                enabler.ScreenEntered += InstantiateSystem;

                void InstantiateSystem()
                {
                    enabler.ScreenEntered -= InstantiateSystem;

                    ThreadPool.QueueUserWorkItem(_ => {
                        var systemInstance = SolarSystemScene.Instantiate<SolarSystemNode>();

                        systemInstance.system = system;
                        systemInstance.MapScale = MapScale;
                        systemInstance.WaypointMapScale = WaypointMapScale;

                        ThreadSafe.Run(systemInstance => enabler.AddChild(systemInstance), systemInstance);
                    });
                }

                ThreadSafe.Run(enabler => AddChild(enabler), enabler);
			}
		});
	}
}
