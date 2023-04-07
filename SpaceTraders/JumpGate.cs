using System.Threading.Tasks;

namespace CosmosPeddler;

public partial class JumpGate
{
    private static CacheDictionary<string, CacheDictionary<string, JumpGate?>> jumpGates = new();

    public static async Task<JumpGate?> GetJumpGate(string systemSymbol, string waypointSymbol)
    {
        if (jumpGates.TryGetValue(systemSymbol, out var cachedSystemJumpGates))
        {
            if (cachedSystemJumpGates.TryGetValue(waypointSymbol, out var cachedJumpGate))
            {
                return cachedJumpGate;
            }
        }
        else jumpGates[systemSymbol] = new();

        var jumpGate = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.GetJumpGateAsync(systemSymbol, waypointSymbol)
        )).Data;

        jumpGates[systemSymbol][waypointSymbol] = jumpGate;

        return jumpGate;
    }
}