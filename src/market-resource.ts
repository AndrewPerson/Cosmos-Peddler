import { Resource } from "./resource";
import { Client } from "./client";

export class MarketResource extends Resource {
    pricePerUnit: number;
    purchasePricePerUnit: number;
    sellPricePerUnit: number;
    quantityAvailable: number;
    spread: 3;
    symbol: string;
    volumePerUnit: number;

    async Purchase(shipId: string, quantity: number) {
        await Client.PurchaseGood(shipId, this.symbol, quantity);
    }
}