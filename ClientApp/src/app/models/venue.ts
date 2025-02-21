import { Coordinates } from "./coordinates";
import { Item } from "./item";
import { Listing } from "./listing";

export interface Venue extends Item {
    id: number;
    type: 'venue';
    name: string;
    about: string;
    coordinates: Coordinates
    imageUrl: string;
    county: string;
    town: string;
    approved: boolean;
}