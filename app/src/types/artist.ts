export interface Genre {
  id: number;
  name: string;
}

export interface ArtistSummary {
  id: number;
  name: string;
  imageUrl: string;
  rating: number;
  genres: Genre[];
}

export interface Artist {
  id: number;
  name: string;
  about?: string;
  imageUrl?: string;
  rating?: number;
  genres: Genre[];
  email?: string;
  latitude?: number;
  longitude?: number;
  county?: string;
  town?: string;
}
