export class AuthenticationError extends Error {
    constructor() {
        super("Invalid SpaceTraders token");
    }
}

export class ResourceError extends Error {}

export class UninitialisedError extends Error {
    constructor() {
        super("Cosmos-Peddler Client is uninitialised");
    }
}