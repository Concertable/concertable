import { Coordinates } from "./coordinates";
import { Listing } from "./listing";

export interface Venue {
    id: number;
    name: string;
    about: string;
    coordinates: Coordinates
    imageUrl: string;
    county: string;
    town: string;
    approved: boolean;
}