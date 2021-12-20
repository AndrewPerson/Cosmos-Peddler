import { Resource } from "../resource";
import { LoanStatus } from "../enums";

export class TakenLoan extends Resource {
    id: string;
    due: Date = new Date();
    repaymentAmount: number;
    status: LoanStatus;
    type: string;
}