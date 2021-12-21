import { Resource } from "./resource";
export declare class MarketResource extends Resource {
    pricePerUnit: number;
    purchasePricePerUnit: number;
    sellPricePerUnit: number;
    quantityAvailable: number;
    spread: 3;
    symbol: string;
    volumePerUnit: number;
    Purchase(shipId: string, quantity: number): Promise<void>;
}
//# sourceMappingURL=market-resource.d.ts.map