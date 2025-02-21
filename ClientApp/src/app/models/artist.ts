import { StringNullableChain } from "lodash";
import { Item } from "./item";

export interface Artist extends Item {
    genres: string[];
    type: 'artist';
    county: string;
    town: string;
}