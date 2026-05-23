import { ArtistHeader } from "./artist-header";
import { EventHeader } from "./event-header";
import { VenueHeader } from "./venue-header";

export interface Header {
    id: number;
    name: string;
    imageUrl: string;
    county: string;
    town: string;
    latitude: number;
    longitude: number;
    rating?: number;
}