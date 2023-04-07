using System.Threading.Tasks;

namespace CosmosPeddler;

public partial class Shipyard
{
    private static CacheDictionary<string, CacheDictionary<string, Shipyard?>> shipyards = new();

    public static async Task<Shipyard?> GetShipyard(string systemSymbol, string waypointSymbol)
    {
        if (shipyards.TryGetValue(systemSymbol, out var cachedSystemShipyards))
        {
            if (cachedSystemShipyards.TryGetValue(waypointSymbol, out var cachedShipyard))
            {
                return cachedShipyard;
            }
        }
        else shipyards[systemSymbol] = new();

        var shipyard = (await SpaceTradersClient.Retry429Policy.ExecuteAsync(
            () => SpaceTradersClient.Client.GetShipyardAsync(systemSymbol, waypointSymbol)
        )).Data;

        shipyards[systemSymbol][waypointSymbol] = shipyard;

        return shipyard;
    }

    public Task<Ship> PurchaseShip(ShipType type) => Ship.PurchaseShip(type, Symbol);
}