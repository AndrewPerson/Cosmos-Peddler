import { Resource } from "../resource";
import { LocationType } from "../enums";

export class LocationInfo extends Resource {
    allowsConstruction: boolean;
    name: string;
    symbol: string;
    type: LocationType;
    x: number;
    y: number;
}