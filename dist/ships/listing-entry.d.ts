import { Resource } from "../resource";
export declare class ShipListingEntry extends Resource {
    class: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    purchaseLocations: ShipPurchaseLocation[];
    speed: number;
    type: string;
    weapons: number;
    Purchase(): Promise<import("./ship").Ship>;
}
export declare type ShipPurchaseLocation = {
    location: string;
    price: number;
    system: string;
};
//# sourceMappingURL=listing-entry.d.ts.map