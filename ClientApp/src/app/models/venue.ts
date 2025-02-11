import { Coordinates } from "./coordinates";
import { CoreEntity } from "./core-entity";
import { Listing } from "./listing";

export interface Venue extends CoreEntity {
    id: number;
    name: string;
    about: string;
    coordinates: Coordinates
    imageUrl: string;
    county: string;
    town: string;
    approved: boolean;
}