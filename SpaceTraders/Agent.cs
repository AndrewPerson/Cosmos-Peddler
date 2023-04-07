using System;
using System.Threading.Tasks;

namespace CosmosPeddler;

public partial class Agent
{
    public static Agent? MyAgent
    {
        get => myAgent;
        set
        {
            myAgent = value;
            OnAgentUpdated?.Invoke(value);
        }
    }

    private static Agent? myAgent;

    public static event Action<Agent?>? OnAgentUpdated;
}