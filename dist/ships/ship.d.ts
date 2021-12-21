import { Resource } from "../resource";
import { FlightPlan } from "../flight-plan/flight-plan";
export declare class Ship extends Resource {
    cargo: Cargo[];
    class: string;
    id: string;
    location: string | undefined;
    manufacturer: string;
    maxCargo: number;
    plating: number;
    spaceAvailable: number;
    speed: 1;
    type: string;
    weapons: 5;
    get flightPlanId(): string | undefined;
    set flightPlanId(value: string | undefined);
    private _flightPlan;
    get x(): number;
    set x(value: number);
    get y(): number;
    set y(value: number);
    private _x;
    private _y;
    destinationX: number | undefined;
    destinationY: number | undefined;
    constructor(object: any);
    FlyTo(locationSymbol: string): Promise<FlightPlan>;
}
export declare class Cargo extends Resource {
    good: string;
    quantity: number;
    totalVolume: number;
    _shipId: string;
    Sell(quantity: number): Promise<void>;
}
//# sourceMappingURL=ship.d.ts.map