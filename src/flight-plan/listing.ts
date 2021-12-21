import { Resource, WriteDate } from "../resource";

export class FlightPlanListing extends Resource {
    @WriteDate
    arrivesAt: Date;

    @WriteDate
    createdAt: Date;

    departure: string;
    destination: string;
    id: string;
    shipId: string;
    shipType: string;
    username: string;
}