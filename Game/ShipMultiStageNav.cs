using Godot;
using System;
using System.Collections.Generic;

using Timer = System.Timers.Timer;

namespace CosmosPeddler.Game;

public record ShipNavStage(string WaypointSymbol, string SystemSymbol, NavMode Mode);

public enum NavMode
{
    Fly,
    Warp,
    Jump
}

public partial class ShipMultiStageNav : Node
{
    public Dictionary<Ship, Queue<ShipNavStage>> ShipNavStages { get; } = new();
    
    public event Action<Ship, ShipNavStage>? OnNavStageStarted;
    public event Action<Ship, ShipNavStage>? OnNavStageFailed;

    public void AddNavStage(Ship ship, ShipNavStage navStage)
    {
        if (!ShipNavStages.TryGetValue(ship, out var navStages))
        {
            ShipNavStages[ship] = navStages = new();
        }

        navStages.Enqueue(navStage);
    }

    private Timer timer;

    public ShipMultiStageNav()
    {
        timer = new Timer(1000)
        {
            AutoReset = true
        };

        timer.Elapsed += (_, _) => CheckShips();
        timer.Start();
    }

    private void CheckShips()
    {
        foreach (var (ship, navStages) in ShipNavStages)
        {
            if (navStages.Count == 0)
            {
                continue;
            }

            if (ship.Nav.Status != ShipNavStatus.IN_TRANSIT)
            {
                var navStage = navStages.Dequeue();

                OnNavStageStarted?.Invoke(ship, navStage);

                if (navStage.Mode == NavMode.Jump)
                {
                    ship.JumpTo(navStage.SystemSymbol).ContinueWith(task =>
                    {
                        if (!task.IsCompletedSuccessfully)
                        {
                            OnNavStageFailed?.Invoke(ship, navStage);
                        }
                    });
                }
                else if (navStage.Mode == NavMode.Warp)
                {
                    ship.WarpTo(navStage.WaypointSymbol).ContinueWith(task =>
                    {
                        if (!task.IsCompletedSuccessfully)
                        {
                            OnNavStageFailed?.Invoke(ship, navStage);
                        }
                    });
                }
                else if (navStage.Mode == NavMode.Fly)
                {
                    ship.NavigateTo(navStage.WaypointSymbol).ContinueWith(task =>
                    {
                        if (!task.IsCompletedSuccessfully)
                        {
                            OnNavStageFailed?.Invoke(ship, navStage);
                        }
                    });
                }
            }
        }
    }
}