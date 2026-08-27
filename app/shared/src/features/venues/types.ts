import type { ImageFile } from "../../types/image";

export interface Venue {
  id: number;
  name: string;
  about: string;
  bannerUrl: string;
  avatar?: string;
  rating: number;
  county: string;
  town: string;
  email: string;
  latitude: number;
  longitude: number;
}

type VenueRequestFields = Pick<
  Venue,
  "name" | "about" | "latitude" | "longitude"
>;

export interface CreateVenueRequest extends VenueRequestFields {
  banner: ImageFile;
  avatar: ImageFile;
}

export interface UpdateVenueRequest extends VenueRequestFields {
  banner?: ImageFile;
  avatar?: ImageFile;
}

export const Venue = {
  toUpdateRequest(venue: Venue): UpdateVenueRequest {
    return {
      name: venue.name,
      about: venue.about,
      latitude: venue.latitude,
      longitude: venue.longitude,
    };
  },
};
