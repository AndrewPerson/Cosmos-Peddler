import { Resource } from "../resource";

export class FlightPlanListing extends Resource {
    arrivesAt: Date = new Date();
    createdAt: Date = new Date();
    departure: string;
    destination: string;
    id: string;
    shipId: string;
    shipType: string;
    username: string;
}