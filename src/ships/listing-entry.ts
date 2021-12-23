import { Resource, ResourceArray, WriteWith } from "../resource";
import { Client } from "../client";

export class ShipListingEntry extends Resource {
    class: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;

    @WriteWith((object: any) => ResourceArray<ShipPurchaseLocation>(object, ShipPurchaseLocation))
    purchaseLocations: ShipPurchaseLocation[];

    speed: number;
    type: string;
    weapons: number;

    async Purchase(locationSymbol: string) {
        return await Client.PurchaseShip(locationSymbol, this.type);
    }
}

export class ShipPurchaseLocation extends Resource {
    location: string;
    price: number;
    system: string;

    async PurchaseShip(shipType: string) {
        return await Client.PurchaseShip(this.location, shipType);
    }
}