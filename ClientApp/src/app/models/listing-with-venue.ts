import { Listing } from "./listing";
import { Venue } from "./venue";

export interface ListingWithVenue {
  listing: Listing;
  venue: Venue;
}
