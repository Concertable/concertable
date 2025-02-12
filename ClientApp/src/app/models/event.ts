import { Artist } from "./artist";
import { ArtistHeader } from "./artist-header";
import { CoreEntity } from "./core-entity";
import { Venue } from "./venue";
import { VenueHeader } from "./venue-header";

export interface Event {
    id: number;
    name: string;
    price: number;
    startDate: Date;
    endDate: Date;
    totalTickets: number;
    availableTickets: number;
    venue: Venue;
    artist: Artist
}