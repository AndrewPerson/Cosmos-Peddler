using Godot;
using System.Threading;
using System.Threading.Tasks;
using CosmosPeddler.Game.SolarSystem;
using CosmosPeddler.Game.SolarSystem.Waypoints;

namespace CosmosPeddler.Game.UI.Navigate;

public partial class NavigateNode : PopupUI<Ship>
{
    [Export]
    public PackedScene SolarSystemScene { get; set; } = null!;

    [Export]
	public float MapScale { get; set; } = 1.0f;

	[Export]
	public float WaypointMapScale { get; set; } = 1.0f;

    private GameCameraNode camera = null!;
    private Node systemsContainer = null!;

    private WaypointNode? hoveredWaypoint = null;

    public override void _EnterTree()
    {
        camera = GetNode<GameCameraNode>("%Camera");
        systemsContainer = GetNode("%Systems");
    }

    public override void _Ready()
    {
        Task.Run(async () =>
        {
            bool centredCamera = false;
            var systems = await CosmosPeddler.SolarSystem.GetSystems();

            foreach (var system in systems)
			{
                if (system == null) continue;

                if (!centredCamera)
                {
                    camera.MapPosition = new Vector2(system.X, system.Y) * MapScale;
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

                        systemInstance.System = system;
                        systemInstance.MapScale = MapScale;
                        systemInstance.WaypointMapScale = WaypointMapScale;
                        
                        systemInstance.WaypointCreated += waypoint => {
                            // TODO
                        };

                        ThreadSafe.Run(systemInstance => enabler.AddChild(systemInstance), systemInstance);
                    });
                }

                ThreadSafe.Run(enabler => AddChild(enabler), enabler);
			}
        });
    }
}
