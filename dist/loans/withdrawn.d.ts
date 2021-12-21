import { Resource } from "../resource";
import { LoanStatus } from "../enums";
export declare class WithdrawnLoan extends Resource {
    id: string;
    due: Date;
    repaymentAmount: number;
    status: LoanStatus;
    type: string;
    Pay(): Promise<void>;
}
//# sourceMappingURL=withdrawn.d.ts.map