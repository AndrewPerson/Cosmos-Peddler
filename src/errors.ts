export class AuthenticationError extends Error {
    constructor() {
        super("Invalid SpaceTraders token");

        this.name = "Authentication Error";
    }
}

export class ResourceError extends Error {
    constructor(message: string) {
        super(message);

        this.name = "Resource Error";
    }
}

export class UninitialisedError extends Error {
    constructor() {
        super("Cosmos-Peddler Client is uninitialised");

        this.name = "Uninitialised Error";
    }
}