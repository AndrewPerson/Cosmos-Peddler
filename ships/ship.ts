import { Resource } from "../resource";

export class Ship extends Resource {
    cargo: Cargo[];
    "class": string;
    flightPlanId: string | undefined;
    id: string;
    location: string;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    spaceAvailable: number;
    speed: 1;
    type: string;
    weapons: 5;
    x: number;
    y: number;
}

export type Cargo = {
    good: string;
    quantity: number;
    totalVolume: number;
}