import { Resource } from "../resource";

export class ShipListingEntry extends Resource {
    "class": string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    purchaseLocations: ShipPurchaseLocation[];
    speed: number;
    type: string;
    weapons: number;
}

export type ShipPurchaseLocation = {
    location: string;
    price: number;
    system: string;
}