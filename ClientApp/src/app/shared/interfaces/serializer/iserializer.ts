export interface ISerializer<T> {
    serialize(obj: Partial<T>, ...args: any[]): any;
    deserialize?(input: any): T;
}