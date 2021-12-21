export declare class Resource {
    [key: string]: any;
    _writers: {
        [key: string]: (value: any) => void;
    };
    constructor(object?: any);
}
export declare function ResourceArray<T extends Resource>(array: any[]): T[];
export declare function WriteDate(target: any, propertyKey: string): void;
export declare function WriteWith(writer: (object: any) => any): (target: any, propertyKey: string) => void;
//# sourceMappingURL=resource.d.ts.map