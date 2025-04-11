import { StringNullableChain } from "lodash";
import { Item } from "./item";
import { Genre } from "./genre";

export interface Artist extends Item {
    genres: Genre[];
    type: 'artist';
    county: string;
    town: string;
    imageUrl: string;
    email: string;
}