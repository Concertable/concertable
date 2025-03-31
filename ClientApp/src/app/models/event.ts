import { Artist } from "./artist";
import { ArtistHeader } from "./artist-header";
import { Genre } from "./genre";
import { Item } from "./item";
import { Venue } from "./venue";
import { VenueHeader } from "./venue-header";

export interface Event extends Item {
    price: number;
    startDate: Date;
    endDate: Date;
    totalTickets: number;
    availableTickets: number;
    venue: Venue;
    artist: Artist;
    datePosted?: Date;
    genres: Genre[];
}