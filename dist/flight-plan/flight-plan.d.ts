import { Resource } from "../resource";
export declare class FlightPlan extends Resource {
    arrivesAt: Date;
    createdAt: Date;
    departure: string;
    destination: string;
    distance: number;
    fuelConsumed: number;
    fuelRemaining: number;
    id: string;
    shipId: string;
    terminatedAt: Date;
    timeRemainingInSeconds: number;
    get progress(): number;
}
//# sourceMappingURL=flight-plan.d.ts.map