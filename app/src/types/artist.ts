export interface Genre {
  id: number;
  name: string;
}

export interface ArtistSummary {
  id: number;
  name: string;
  avatar?: string;
  rating: number;
  genres: Genre[];
}

export interface Artist {
  id: number;
  name: string;
  about: string;
  bannerUrl: string;
  avatar?: string;
  rating: number;
  genres: Genre[];
  email: string;
  county: string;
  town: string;
}
