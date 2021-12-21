import { Resource, ResourceArray, WriteWith } from "../resource";
import { FlightPlan } from "../flight-plan/flight-plan";
import { Client } from "../client";

export class Ship extends Resource {
    @WriteWith((object: any) => ResourceArray<Cargo>(object))
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

    get flightPlanId() {
        return this._flightPlan?.id;
    }

    set flightPlanId(value: string | undefined) {
        if (value === undefined) {
            this._flightPlan = undefined;
            return;
        }

        Client.FlightPlan(value).then(async flightPlan => {
            this._flightPlan = flightPlan;

            var location = await Client.LocationInfo(flightPlan.destination);
            this.destinationX = location.x;
            this.destinationY = location.y;
        });
    }

    private _flightPlan: FlightPlan | undefined;

    get x() {
        if (this.flightPlanId === undefined ||
            this.destinationX === undefined ||
            this.destinationY === undefined) return this._x;

        return this._x + (this._x - this.destinationX) * (this._flightPlan?.progress ?? 0);
    }

    set x(value: number) {
        if (this.flightPlanId === undefined) this._x = value;
    }

    get y() {
        if (this.flightPlanId === undefined ||
            this.destinationX === undefined ||
            this.destinationY === undefined) return this._y;

        return this._y + (this._y - this.destinationY) * (this._flightPlan?.progress ?? 0);
    }

    set y(value: number) {
        if (this.flightPlanId === undefined) this._y = value;
    }

    private _x: number;
    private _y: number;

    destinationX: number | undefined;
    destinationY: number | undefined;

    constructor(object: any) {
        super(object);

        for (var c of this.cargo) {
            c._shipId = this.id;
        }
    }

    async FlyTo(locationSymbol: string) {
        return await Client.CreateFlightPlan(this.id, locationSymbol);
    }
}

export class Cargo extends Resource {
    good: string;
    quantity: number;
    totalVolume: number;

    _shipId: string;

    Sell(quantity: number) {
        return Client.SellGood(this._shipId, this.good, quantity);
    }
}