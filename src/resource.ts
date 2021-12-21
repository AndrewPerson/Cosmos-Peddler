export class Resource {
    [key:string]: any;

    _writers: { [key:string]: (value: any) => void } = {};

    constructor(object: any = undefined) {
        if (object == null || object == undefined) return;

        for (var key of Object.keys(object)) {
            if (key in this._writers) this[key] = this._writers[key](object[key]);
            else this[key] = object[key];
        }
    }
}

export function ResourceArray<T extends Resource>(array: any[]): T[] {
    return array.map((item: any) => <T>(item));
}

export function WriteDate(target: any, propertyKey: string) {
    target._writers[propertyKey] = (value: any) => new Date(value);
}

export function WriteWith(writer: (object: any) => any) {
    return (target: any, propertyKey: string) => {
        target._writers[propertyKey] = writer;
    };
}