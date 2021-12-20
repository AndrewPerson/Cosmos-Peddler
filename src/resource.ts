export class Resource {
    [key:string]: any;

    constructor(object: any = undefined) {
        if (object == null || object == undefined) return;

        for (var key of Object.keys(object)) {
            if (this[key] instanceof Date) this[key] = new Date(object[key]);
            else this[key] = object[key];
        }
    }
}

export function ResourceArray<T extends Resource>(array: any[]): T[] {
    return array.map((item: any) => <T>(item));
}