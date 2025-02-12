import { ArtistHeader } from "./artist-header";
import { EventHeader } from "./event-header";
import { VenueHeader } from "./venue-header";

export interface Header {
    id: number;
    name: string;
    about: string;
    imageUrl: string;
}