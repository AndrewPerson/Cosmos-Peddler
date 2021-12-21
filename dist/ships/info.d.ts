import { Resource } from "../resource";
export declare class ShipInfo extends Resource {
    class: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    speed: number;
    type: string;
    weapons: number;
    Purchase(): Promise<import("./ship").Ship>;
}
//# sourceMappingURL=info.d.ts.map