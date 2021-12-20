import { Resource, ResourceArray } from "./resource";

export class LeaderBoard extends Resource {
    leaderBoard: LeaderBoardEntry[];
    you: LeaderBoardEntry;

    constructor(object: any) {
        super();

        this.leaderBoard = ResourceArray(object.netWorth);
        this.you = new LeaderBoardEntry(object.userNetWorth[0]);
    }
}

export class LeaderBoardEntry extends Resource {
    netWorth: number;
    rank: number;
    username: string;
}