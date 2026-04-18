import type { Genre } from "@/types/common";

export type HeaderType = "artist" | "venue" | "concert";

export interface Header {
  $type: HeaderType;
  id: number;
  name: string;
  imageUrl: string;
  rating?: number;
  county: string;
  town: string;
}

export interface ArtistHeader extends Header {
  genres: Genre[];
}

export interface VenueHeader extends Header {}

export interface ConcertHeader extends Header {
  startDate: string;
  endDate: string;
  datePosted?: string;
  genres: Genre[];
}
