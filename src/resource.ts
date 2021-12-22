export class Resource {
    [key:string]: any;

    constructor(object: any = undefined) {
        if (object == null || object == undefined) return;

        for (var key of Object.keys(object)) {
            var value = this._writers !== undefined && key in this._writers ?
                        this._writers[key](object[key]) :
                        object[key];

            var propertyKey = this._remaps !== undefined && key in this._remaps ?
                              this._remaps[key] :
                              key;

            this[propertyKey] = value;
        }
    }
}

export function ResourceArray<T extends Resource>(array: any[], type: typeof Resource): T[] {
    return array.map((item: any) => <T> new type(item));
}

export function WriteDate(target: any, propertyKey: string) {
    if (target._writers === undefined) target._writers = {};

    target._writers[propertyKey] = (value: any) => value === null || value === undefined ? value : new Date(value);
}

export function WriteWith(writer: (object: any) => any) {
    return (target: any, propertyKey: string) => {
        if (target._writers === undefined) target._writers = {};

        target._writers[propertyKey] = writer;
    };
}

export function Remap(to: string) {
    return (target: any, propertyKey: string) => {
        if (target._remaps === undefined) target._remaps = {};

        target._remaps[propertyKey] = to;
    };
}