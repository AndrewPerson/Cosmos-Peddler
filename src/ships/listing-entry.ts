import { Resource } from "../resource";
import { Client } from "../client";

export class ShipListingEntry extends Resource {
    class: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    purchaseLocations: ShipPurchaseLocation[];
    speed: number;
    type: string;
    weapons: number;

    async Purchase() {
        return await Client.PurchaseShip(this.type, this.class);
    }
}

export type ShipPurchaseLocation = {
    location: string;
    price: number;
    system: string;
}