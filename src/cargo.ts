import { Resource } from "./resource";
import { Client } from "./client";

export class Cargo extends Resource {
    good: string;
    quantity: number;
    totalVolume: number;

    _shipId: string;

    async Sell(quantity: number) {
        return await Client.SellGood(this._shipId, this.good, quantity);
    }

    async TransferTo(toShipId: string, quantity: number) {
        return await Client.TransferCargo(this._shipId, toShipId, this.good, quantity);
    }
}