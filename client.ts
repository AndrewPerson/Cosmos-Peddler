import { ResourceError, AuthenticationError, NotFoundError } from "./errors";
import { HTTPMethod } from "./enums";

import { Resource, ResourceArray } from "./resource";

import { AvailableLoan } from "./loans/available";
import { TakenLoan } from "./loans/taken";
import { LeaderBoard } from "./leaderboard";
import { ShipListingEntry } from "./ships/listing-entry";
import { FlightPlanListing } from "./flight-plan/listing";
import { DockedShip } from "./ships/docked";
import { SystemLocation } from "./system/system";
import { SystemInfo } from "./system/info";
import { LocationInfo } from "./locations/info";
import { MarketResource } from "./market-resource";
import { ShipInfo } from "./ships/info";
import { FlightPlan } from "./flight-plan/flight-plan";

export class Account extends Resource {
    static async Register(name: string): Promise<string> {
        var userResponse = await fetch(`http://api.spacetraders.io/users/${name}/claim`, {
            method: "POST"
        });

        if (!userResponse.ok) throw new ResourceError(userResponse.statusText);

        return (await userResponse.json()).token;
    }

    static async Instantiate(token: string): Promise<Account> {
        var userResponse = await fetch("https://api.spacetraders.io/my/account", {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });

        if (!userResponse.ok) throw new ResourceError(userResponse.statusText);

        return new Account({
            ...await userResponse.json(),
            token: token
        });
    }

    token: string;

    username: string;
    shipCount: number;
    structureCount: number;
    joinedAt: Date = new Date();
    credits: number;

    //TODO Cache ships. (Not necessarily here.)

    rateLimited: boolean = false;

    requestQueue: {
        resource: string;
        method: HTTPMethod;
        options: Record<string, string>;
        onResolve: (resource: any) => void;
        onReject: (reason: any) => void;
    }[] = [];

    //#region GetResources
    private async GetResourceRaw(resource: string, method: HTTPMethod, options: Record<string, string>) {
        if (method == HTTPMethod.GET) {
            var resourceResponse = await fetch(`https://api.spacetraders.io/${resource}?${new URLSearchParams(options).toString()}`, {
                method: method,
                headers: {
                    Authorization: `Bearer ${this.token}`
                }
            });
        }
        else {
            var resourceResponse = await fetch(`https://api.spacetraders.io/${resource}`, {
                method: method,
                headers: {
                    Authorization: `Bearer ${this.token}`
                },
                body: options as any
            });
        }

        if (resourceResponse.status == 401 || resourceResponse.status == 403) throw new AuthenticationError("Invalid token");
        if (resourceResponse.status == 404) throw new NotFoundError(resourceResponse.statusText);
        if (!resourceResponse.ok) throw new ResourceError(resourceResponse.statusText);

        return resourceResponse;
    }

    private async GetResource(resource: string, method: HTTPMethod, options: Record<string, string> = {}) {
        var resourceResponse = await this.GetResourceRaw(resource, method, options);

        if (resourceResponse.status == 429) {
            this.rateLimited = true;

            var result = new Promise((resolve, reject) => {
                this.requestQueue.push({
                    resource: resource,
                    method: method,
                    options: options,
                    onResolve: resolve,
                    onReject: reject
                });
            });

            this.ScheduleRequestRetry(parseFloat(resourceResponse.headers.get("retry-after") || "1") * 1000);

            return result;
        }

        return await resourceResponse.json();
    }

    private ScheduleRequestRetry(delay: number) {
        //Add an extra 10ms to give leeway in case clocks aren't aligned
        setTimeout(this.SendQueuedRequests.bind(this), delay + 10);
    }

    private async SendQueuedRequests(): Promise<void> {
        var requestCount = this.requestQueue.length;

        for (var i = 0; i < requestCount; i++) {
            var request = this.requestQueue[0];

            try {
                var resourceResponse = await this.GetResourceRaw(request.resource, request.method, request.options);
            }
            catch (e) {
                request.onReject(e);
                continue;
            }

            if (resourceResponse.status == 429) {
                this.ScheduleRequestRetry(parseFloat(resourceResponse.headers.get("retry-after") || "1") * 1000);

                return;
            }

            request.onResolve(await resourceResponse.json());
            this.requestQueue.shift();
        }

        this.rateLimited = false;
    }
    //#endregion

    //#region Loans
    async AvailableLoans(): Promise<AvailableLoan[]> {
        return ResourceArray<AvailableLoan>((await this.GetResource("types/loans", HTTPMethod.GET)).loans);
    }

    async TakenLoans(): Promise<TakenLoan[]> {
        return ResourceArray<TakenLoan>((await this.GetResource("my/loans", HTTPMethod.GET)).loans);
    }

    async TakeLoan(type: string): Promise<TakenLoan> {
        var loanResponse = await this.GetResource("my/loans", HTTPMethod.POST, {
            type: type
        });

        this.credits = loanResponse.credits;

        return new TakenLoan(loanResponse.loan);
    }

    async PayLoan(loan: TakenLoan): Promise<TakenLoan[]> {
        var loanResponse = await this.GetResource(`my/loans/${loan.id}`, HTTPMethod.PUT);

        this.credits = loanResponse.credits;

        return ResourceArray<TakenLoan>(loanResponse.loans);
    }
    //#endregion

    //#region LeaderBoard
    async LeaderBoard(): Promise<LeaderBoard> {
        return new LeaderBoard(await this.GetResource("game/leaderboard/net-worth", HTTPMethod.GET));
    }
    //#endregion

    //#region Systems
    async SystemLocations(systemSymbol: string): Promise<SystemLocation[]> {
        return ResourceArray<SystemLocation>((await this.GetResource(`systems/${systemSymbol}/locations`, HTTPMethod.GET)).locations);
    }

    async SystemInfo(systemSymbol: string): Promise<SystemInfo> {
        return new SystemInfo((await this.GetResource(`systems/${systemSymbol}`, HTTPMethod.GET)).system);
    }
    //#endregion

    //#region Locations
    async LocationInfo(locationSymbol: string): Promise<LocationInfo> {
        return new LocationInfo((await this.GetResource(`locations/${locationSymbol}`, HTTPMethod.GET)).location);
    }
    //#endregion

    //#region Ships
    async AvailableShips(): Promise<ShipInfo[]> {
        return ResourceArray<ShipInfo>((await this.GetResource("types/ships", HTTPMethod.GET)).ships);
    }

    async ShipListings(systemSymbol: string): Promise<ShipListingEntry[]> {
        return ResourceArray<ShipListingEntry>((await this.GetResource(`systems/${systemSymbol}/ship-listings`, HTTPMethod.GET)).shipListings);
    }

    async SystemDockedShips(systemSymbol: string): Promise<DockedShip[]> {
        return ResourceArray<DockedShip>((await this.GetResource(`systems/${systemSymbol}/ships`, HTTPMethod.GET)).ships);
    }

    async FlightPlans(systemSymbol: string): Promise<FlightPlanListing[]> {
        return ResourceArray<FlightPlanListing>((await this.GetResource(`systems/${systemSymbol}/flight-plans`, HTTPMethod.GET)).flightPlans);
    }

    async FlightPlan(flightPlanId: string): Promise<FlightPlan> {
        return new FlightPlan((await this.GetResource(`my/flight-plans/${flightPlanId}`, HTTPMethod.GET)).flightPlan);
    }

    async CreateFlightPlan(shipId: string, destination: string): Promise<FlightPlan> {
        return new FlightPlan((await this.GetResource(`my/flight-plans`, HTTPMethod.POST, {
            shipId: shipId,
            destination: destination
        })).flightPlan);
    }

    async LocationShips(locationSymbol: string): Promise<DockedShip[]> {
        return ResourceArray<DockedShip>((await this.GetResource(`locations/${locationSymbol}/ships`, HTTPMethod.GET)).ships);
    }
    //#endregion

    //#region Goods
    async Marketplace(locationSymbol: string): Promise<MarketResource[]> {
        return ResourceArray<MarketResource>((await this.GetResource(`locations/${locationSymbol}/marketplace`, HTTPMethod.GET)).marketplace);
    }

    async Purchase(shipId: string, good: string, quantity: number): Promise<void> {
        var purchaseResponse = await this.GetResource("my/purchase-orders", HTTPMethod.POST, {
            shipId: shipId,
            good: good,
            quantity: quantity.toString()
        });

        this.credits = purchaseResponse.credits;

        //TODO update cached ships based on ship returned in result.
    }

    async Sell(shipId: string, good: string, quantity: number): Promise<void> {
        var sellResponse = await this.GetResource("my/sell-orders", HTTPMethod.POST, {
            shipId: shipId,
            good: good,
            quantity: quantity.toString()
        });

        this.credits = sellResponse.credits;
    }
    //#endregion
}