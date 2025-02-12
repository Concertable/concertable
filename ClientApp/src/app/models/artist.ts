import { CoreEntity } from "./core-entity";

export interface Artist extends CoreEntity {
    genres: string[];
    county: string;
    town: string;
}