import type { Genre } from "@/types/common";
import type { Contract } from "@/features/contracts";

export interface Opportunity {
  id: number;
  venueId: number;
  startDate: string;
  endDate: string;
  genres: Genre[];
  contract: Contract;
}

export interface ConcertArtist {
  id: number;
  name: string;
  avatar?: string;
  rating: number;
  county: string;
  town: string;
  genres: Genre[];
}

export interface ConcertVenue {
  id: number;
  name: string;
  county: string;
  town: string;
  latitude: number;
  longitude: number;
}

export interface Concert {
  id: number;
  name: string;
  about: string;
  bannerUrl: string;
  avatar: string;
  rating: number;
  price: number;
  totalTickets: number;
  availableTickets: number;
  startDate: string;
  endDate: string;
  datePosted?: string;
  venue: ConcertVenue;
  artist: ConcertArtist;
  genres: Genre[];
}
