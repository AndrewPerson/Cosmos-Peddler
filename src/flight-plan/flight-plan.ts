import { Resource, WriteDate, Remap } from "../resource";
import { Client } from "../client";

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

    @Remap("staleTimeRemainingInSeconds")
    get timeRemainingInSeconds() {
        return Math.min(Math.floor((this.arrivesAt.getTime() - new Date().getTime()) / 1000), 0);
    }

    staleTimeRemainingInSeconds: number;

    get progress() {
        return 1 - this.Clamp(0, 1, (this.arrivesAt.getTime() - new Date().getTime()) / (this.arrivesAt.getTime() - this.createdAt.getTime()));
    }

    constructor(object: any = undefined) {
        super(object);

        if (this.terminatedAt === null) {
            setTimeout(() => {
                this.Refresh();
            // Subtract 2 seconds to help mitigate the time of the network round-trip
            }, this.arrivesAt.getTime() - new Date().getTime() - 2000);
        }
    }

    Clamp(min: number, max: number, value: number) {
        return value > max ? max : value < min ? min : value;
    }

    async Refresh() {
        var flightPlan = await Client.UncachedFlightPlan(this.id);

        if (flightPlan.terminatedAt === null) {
            this.arrivesAt.setSeconds(this.arrivesAt.getSeconds() + flightPlan.staleTimeRemainingInSeconds);

            setTimeout(() => {
                this.Refresh();
            }, this.arrivesAt.getTime() - new Date().getTime());

            return;
        }

        this.terminatedAt = flightPlan.terminatedAt;
    }
}