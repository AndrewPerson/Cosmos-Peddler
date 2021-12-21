import{Resource as e}from"../resource";import{Client as a}from"../client";export class AvailableLoan extends e{async Withdraw(){await a.WithdrawLoan(this.type)}}
