import{Resource as i}from"./resource";import{Client as n}from"./client";export class MarketResource extends i{async Purchase(e,r){await n.PurchaseGood(e,this.symbol,r)}}
