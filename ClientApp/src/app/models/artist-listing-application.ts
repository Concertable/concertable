import { Artist } from "./artist";
import { ListingWithVenue } from "./listing-with-venue";

export interface ArtistListingApplication {
    id: number;
    artist: Artist;
    listingWithVenue: ListingWithVenue;
}