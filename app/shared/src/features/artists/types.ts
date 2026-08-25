import type { Genre } from "../../types/common";
import type { ImageFile } from "../../types/image";

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
  latitude: number;
  longitude: number;
}

type ArtistRequestFields = Pick<
  Artist,
  "name" | "about" | "latitude" | "longitude" | "genres"
>;

export interface CreateArtistRequest extends ArtistRequestFields {
  banner: ImageFile;
  avatar: ImageFile;
}

export interface UpdateArtistRequest extends ArtistRequestFields {
  banner?: ImageFile;
  avatar?: ImageFile;
}

export const Artist = {
  toUpdateRequest(artist: Artist): UpdateArtistRequest {
    return {
      name: artist.name,
      about: artist.about,
      latitude: artist.latitude,
      longitude: artist.longitude,
      genres: artist.genres,
    };
  },
};
