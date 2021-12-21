import { Resource, WriteDate } from "./resource";

export class User extends Resource {
    credits: number;

    @WriteDate
    joinedAt: Date;
    
    shipCount: number;
    structureCount: number;
    username: string;
}