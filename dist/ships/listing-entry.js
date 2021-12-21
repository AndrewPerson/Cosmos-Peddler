import{Resource as r}from"../resource";import{Client as e}from"../client";export class ShipListingEntry extends r{async Purchase(){return await e.PurchaseShip(this.type,this.class)}}
