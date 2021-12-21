import { Resource, WriteDate } from "../resource";

export class FlightPlan extends Resource {
    @WriteDate
    arrivesAt: Date;

    @WriteDate
    createdAt: Date;

    departure: string;
    destination: string;
    distance: number;
    fuelConsumed: number;
    fuelRemaining: number;
    id: string;
    shipId: string;

    @WriteDate
    terminatedAt: Date;
    
    timeRemainingInSeconds: number;

    get progress() {
        return 1 - (this.arrivesAt.getTime() - new Date().getTime()) / (this.arrivesAt.getTime() - this.createdAt.getTime());
    }
}