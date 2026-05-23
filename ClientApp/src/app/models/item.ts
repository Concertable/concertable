import { ItemType } from "./item-type";

export interface Item {
    id: number;
    type: ItemType;
    name: string;
    about: string;
    rating: number;
}