import type { Venue } from "@/types/venue";
import type { Artist, Genre } from "@/types/artist";

export interface Concert {
  id: number;
  name: string;
  about: string;
  rating: number;
  price: number;
  totalTickets: number;
  availableTickets: number;
  startDate: string;
  endDate: string;
  datePosted?: string;
  venue: Venue;
  artist: Artist;
  genres: Genre[];
}
