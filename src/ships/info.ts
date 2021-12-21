import { Resource } from "../resource";
import { Client } from "../client";

export class ShipInfo extends Resource {
    class: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    speed: number;
    type: string;
    weapons: number;

    async Purchase() {
        return await Client.PurchaseShip(this.type, this.class);
    }
}