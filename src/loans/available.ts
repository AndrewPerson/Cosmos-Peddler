import { Resource } from "../resource";
import { Client } from "../client";

export class AvailableLoan extends Resource {
    type: string;
    amount: number;
    rate: number;
    termInDays: number;
    collateralRequired: boolean;

    async Withdraw() {
        await Client.WithdrawLoan(this.type);
    }
}