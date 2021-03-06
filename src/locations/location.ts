import { Resource } from "../resource";
import { LocationType, LocationTrait } from "../enums";

export class Location extends Resource {
    allowsConstruction: boolean;
    dockedShips: number;
    name: string;
    symbol: string;
    type: LocationType;
    traits: LocationTrait[];
    x: number;
    y: number;
}