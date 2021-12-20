import { Resource } from "./resource";

export class MarketResource extends Resource {
    pricePerUnit: number;
    purchasePricePerUnit: number;
    sellPricePerUnit: number;
    quantityAvailable: number;
    spread: 3;
    symbol: string;
    volumePerUnit: number;
}