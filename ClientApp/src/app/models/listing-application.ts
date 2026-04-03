import { Artist } from "./artist";
import { Listing } from "./listing";

export interface ListingApplication {
    id: number;
    artist: Artist;
    listing: Listing;
}