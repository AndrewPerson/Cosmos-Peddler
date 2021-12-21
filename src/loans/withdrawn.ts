import { Resource, WriteDate } from "../resource";
import { LoanStatus } from "../enums";
import { Client } from "../client";

export class WithdrawnLoan extends Resource {
    id: string;

    @WriteDate
    due: Date;

    repaymentAmount: number;
    status: LoanStatus;
    type: string;

    async Pay() {
        await Client.PayLoan(this.id);

        var newThis = (await Client.WithdrawnLoans()).find(loan => loan.id == this.id);

        // Just in case. newThis should always be defined though.
        if (newThis === undefined) this.status = LoanStatus.Paid;
        else this.status = newThis.status
    }
}