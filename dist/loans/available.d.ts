import { Resource } from "../resource";
export declare class AvailableLoan extends Resource {
    type: string;
    amount: number;
    rate: number;
    termInDays: number;
    collateralRequired: boolean;
    Withdraw(): Promise<void>;
}
//# sourceMappingURL=available.d.ts.map