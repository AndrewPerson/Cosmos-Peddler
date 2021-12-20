import { Resource } from "../resource";

export class AvailableLoan extends Resource {
    type: string;
    amount: number;
    rate: number;
    termInDays: number;
    collateralRequired: boolean;
}