import { ResourceError, AuthenticationError, NotFoundError } from "./errors";
import { HTTPMethod } from "./enums";

import { Resource, ResourceArray } from "./resource";

import { User } from "./user";
import { AvailableLoan } from "./loans/available";
import { WithdrawnLoan } from "./loans/withdrawn";
import { LeaderBoard } from "./leaderboard";
import { ShipListingEntry } from "./ships/listing-entry";
import { FlightPlanListing } from "./flight-plan/listing";
import { DockedShip } from "./ships/docked";
import { LocationInfo } from "./locations/info";
import { System } from "./system";
import { Location } from "./locations/location";
import { MarketResource } from "./market-resource";
import { ShipInfo } from "./ships/info";
import { FlightPlan } from "./flight-plan/flight-plan";
import { Ship } from "./ships/ship";

export class Client extends Resource {
    static async Register(username: string): Promise<string> {
        var userResponse = await fetch(`http://api.spacetraders.io/users/${username}/claim`, {
            method: "POST"
        });

        if (!userResponse.ok) throw new ResourceError(userResponse.statusText);

        return (await userResponse.json()).token;
    }

    static async Initialise(token: string) {
        this.token = token;
        this.initialised = true;
    }

    static initialised: boolean = false;
    static token: string;

    static user: User | undefined = undefined;

    static withdrawnLoans: WithdrawnLoan[] | undefined = undefined;

    static ships: Ship[] | undefined = undefined;

    static systemFlightPlans: {
        [key: string]: FlightPlanListing[]
    } = {};

    static flightPlans: FlightPlan[] | undefined = undefined;

    static rateLimited: boolean = false;

    static requestQueue: {
        resource: string;
        method: HTTPMethod;
        options: Record<string, string>;
        onResolve: (resource: any) => void;
        onReject: (reason: any) => void;
    }[] = [];

    //#region GetResources
    private static async GetResourceRaw(resource: string, method: HTTPMethod, options: Record<string, string>) {
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

    private static async GetResource(resource: string, method: HTTPMethod, options: Record<string, string> = {}) {
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

    private static ScheduleRequestRetry(delay: number) {
        //Add an extra 10ms to give leeway in case clocks aren't aligned
        setTimeout(this.SendQueuedRequests.bind(this), delay + 10);
    }

    private static async SendQueuedRequests(): Promise<void> {
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

    //#region User
    static async User(): Promise<User> {
        if (this.user === undefined) {
            this.user = new User(await this.GetResource("my/account", HTTPMethod.GET));
        }

        return this.user;
    }
    //#endregion

    //#region Loans
    static async AllLoans(): Promise<AvailableLoan[]> {
        return ResourceArray<AvailableLoan>((await this.GetResource("types/loans", HTTPMethod.GET)).loans);
    }

    static async AvailableLoans(): Promise<AvailableLoan[]> {
        var withdrawnLoans = await this.WithdrawnLoans();
        var allLoans = await this.AllLoans();

        return allLoans.filter(loan => !withdrawnLoans.some(withdrawnLoan => withdrawnLoan.type == loan.type));
    }

    static async WithdrawnLoans(): Promise<WithdrawnLoan[]> {
        if (this.withdrawnLoans === undefined) {
            this.withdrawnLoans = ResourceArray<WithdrawnLoan>((await this.GetResource("my/loans", HTTPMethod.GET)).loans);
        }
        
        return this.withdrawnLoans;
    }

    static async WithdrawLoan(type: string): Promise<WithdrawnLoan> {
        var loanResponse = await this.GetResource("my/loans", HTTPMethod.POST, {
            type: type
        });

        if (this.user !== undefined) this.user.credits = loanResponse.credits;

        if (this.withdrawnLoans !== undefined) this.withdrawnLoans.push(new WithdrawnLoan(loanResponse.loan));

        return new WithdrawnLoan(loanResponse.loan);
    }

    static async PayLoan(loanId: string): Promise<void> {
        var loanResponse = await this.GetResource(`my/loans/${loanId}`, HTTPMethod.PUT);

        if (this.user !== undefined) this.user.credits = loanResponse.credits;

        this.withdrawnLoans = ResourceArray<WithdrawnLoan>(loanResponse.loans);
    }
    //#endregion

    //#region LeaderBoard
    static async LeaderBoard(): Promise<LeaderBoard> {
        return new LeaderBoard(await this.GetResource("game/leaderboard/net-worth", HTTPMethod.GET));
    }
    //#endregion

    //#region Systems
    static async SystemInfo(systemSymbol: string): Promise<System> {
        return new System((await this.GetResource(`systems/${systemSymbol}`, HTTPMethod.GET)).system);
    }
    //#endregion

    //#region Locations
    static async SystemLocations(systemSymbol: string): Promise<LocationInfo[]> {
        return ResourceArray<LocationInfo>((await this.GetResource(`systems/${systemSymbol}/locations`, HTTPMethod.GET)).locations);
    }

    static async LocationInfo(locationSymbol: string): Promise<Location> {
        return new Location((await this.GetResource(`locations/${locationSymbol}`, HTTPMethod.GET)).location);
    }
    //#endregion

    //#region Ships
    static async AvailableShips(): Promise<ShipInfo[]> {
        return ResourceArray<ShipInfo>((await this.GetResource("types/ships", HTTPMethod.GET)).ships);
    }

    static async ShipListings(systemSymbol: string): Promise<ShipListingEntry[]> {
        return ResourceArray<ShipListingEntry>((await this.GetResource(`systems/${systemSymbol}/ship-listings`, HTTPMethod.GET)).shipListings);
    }

    static async MyShips(): Promise<Ship[]> {
        if (this.ships === undefined) {
            this.ships = ResourceArray<Ship>((await this.GetResource("my/ships", HTTPMethod.GET)).ships);
        }

        return this.ships;
    }

    static async PurchaseShip(location: string, type: string): Promise<Ship> {
        var shipResponse = await this.GetResource("my/ships", HTTPMethod.POST, {
            location: location,
            type: type
        });

        var ship = new Ship(shipResponse.ship);

        if (this.ships === undefined) {
            this.ships = [ship];
        }
        else {
            this.ships.push(ship);
        }

        if (this.user !== undefined) this.user.credits = shipResponse.credits;

        return ship;
    }

    static async SystemDockedShips(systemSymbol: string): Promise<DockedShip[]> {
        return ResourceArray<DockedShip>((await this.GetResource(`systems/${systemSymbol}/ships`, HTTPMethod.GET)).ships);
    }

    static async FlightPlans(systemSymbol: string): Promise<FlightPlanListing[]> {
        if (!(systemSymbol in this.systemFlightPlans)) {
            this.systemFlightPlans[systemSymbol] = ResourceArray<FlightPlanListing>((await this.GetResource(`systems/${systemSymbol}/flight-plans`, HTTPMethod.GET)).flightPlans);
        }
        
        return this.systemFlightPlans[systemSymbol];
    }

    static async FlightPlan(flightPlanId: string): Promise<FlightPlan> {
        if (this.flightPlans === undefined) {
            return new FlightPlan((await this.GetResource(`my/flight-plans/${flightPlanId}`, HTTPMethod.GET)).flightPlan);
        }

        var flightPlan = this.flightPlans.find(flightPlan => flightPlan.id == flightPlanId);

        if (flightPlan === undefined) {
            return new FlightPlan((await this.GetResource(`my/flight-plans/${flightPlanId}`, HTTPMethod.GET)).flightPlan);
        }

        return flightPlan;
    }

    static async CreateFlightPlan(shipId: string, locationSymbol: string): Promise<FlightPlan> {
        var flightPlan = new FlightPlan((await this.GetResource(`my/flight-plans`, HTTPMethod.POST, {
            shipId: shipId,
            destination: locationSymbol
        })).flightPlan);

        if (this.flightPlans === undefined) {
            this.flightPlans = [flightPlan];
        }
        else {
            this.flightPlans.push(flightPlan);
        }

        if (this.ships !== undefined) {
            var index = this.ships.findIndex(ship => ship.id == shipId);
            
            if (index != -1) {
                this.ships[index].flightPlanId = flightPlan.id;
                this.ships[index].location = undefined;
                
                var fuelIndex = this.ships[index].cargo.findIndex(cargo => cargo.good == "FUEL");

                if (fuelIndex != -1) {
                    this.ships[index].cargo[fuelIndex].quantity = flightPlan.fuelRemaining;
                }
            }
        }

        return flightPlan;
    }

    static async LocationShips(locationSymbol: string): Promise<DockedShip[]> {
        return ResourceArray<DockedShip>((await this.GetResource(`locations/${locationSymbol}/ships`, HTTPMethod.GET)).ships);
    }
    //#endregion

    //#region Goods
    static async Marketplace(locationSymbol: string): Promise<MarketResource[]> {
        return ResourceArray<MarketResource>((await this.GetResource(`locations/${locationSymbol}/marketplace`, HTTPMethod.GET)).marketplace);
    }

    static async PurchaseGood(shipId: string, goodSymbol: string, quantity: number): Promise<void> {
        var purchaseResponse = await this.GetResource("my/purchase-orders", HTTPMethod.POST, {
            shipId: shipId,
            good: goodSymbol,
            quantity: quantity.toString()
        });

        if (this.user !== undefined) this.user.credits = purchaseResponse.credits;

        if (this.ships !== undefined) {
            var ship = new Ship(purchaseResponse.ship);

            var index = this.ships.findIndex(s => s.id == ship.id);

            this.ships[index] = ship;
        }
    }

    static async SellGood(shipId: string, good: string, quantity: number): Promise<void> {
        var sellResponse = await this.GetResource("my/sell-orders", HTTPMethod.POST, {
            shipId: shipId,
            good: good,
            quantity: quantity.toString()
        });

        if (this.user !== undefined) this.user.credits = sellResponse.credits;
    }
    //#endregion
}