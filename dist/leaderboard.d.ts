import { Resource } from "./resource";
export declare class LeaderBoard extends Resource {
    leaderBoard: LeaderBoardEntry[];
    you: LeaderBoardEntry;
    constructor(object: any);
}
export declare class LeaderBoardEntry extends Resource {
    netWorth: number;
    rank: number;
    username: string;
}
//# sourceMappingURL=leaderboard.d.ts.map