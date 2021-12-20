import { Resource } from "../resource";

export class FlightPlan extends Resource {
    arrivesAt: Date = new Date();
    createdAt: Date = new Date();
    departure: string;
    destination: string;
    distance: number;
    fuelConsumed: number;
    fuelRemaining: number;
    id: string;
    shipId: string;
    terminatedAt: Date = new Date();
    timeRemainingInSeconds: number;
}