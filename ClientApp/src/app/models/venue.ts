import { Coordinates } from "./coordinates";
import { Item } from "./item";
import { Listing } from "./listing";

export interface Venue extends Item {
    id: number;
    type: 'venue';
    imageUrl: string;
    county: string;
    town: string;
    latitude: number;
    longitude: number;
    userId: number;
    approved: boolean;
}