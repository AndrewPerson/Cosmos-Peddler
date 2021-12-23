import { Resource } from "../resource";

export class ShipInfo extends Resource {
    class: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    speed: number;
    type: string;
    weapons: number;
}