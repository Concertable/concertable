import { apiClient } from "../../../lib/apiClient";
import type { Venue } from "../types";
import type { ImageFile } from "../../../types/image";

export interface CreateVenue {
  name: string;
  about: string;
  latitude: number;
  longitude: number;
  banner: File;
  avatar: File;
}

const venueApi = {
  getVenue: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`/venue/${id}`);
    return data;
  },

  getOrganizationVenue: async (): Promise<Venue | null> => {
    const { data } = await apiClient.getOptional<Venue>("/organization/venue");
    return data;
  },

  createVenue: async (input: CreateVenue): Promise<Venue> => {
    const formData = new FormData();
    formData.append("Name", input.name);
    formData.append("About", input.about);
    formData.append("Latitude", String(input.latitude));
    formData.append("Longitude", String(input.longitude));
    formData.append("Banner", input.banner);
    formData.append("Avatar", input.avatar);
    const { data } = await apiClient.post<Venue>(
      "/organization/venue",
      formData,
    );
    return data;
  },

  updateVenue: async (
    venue: Venue,
    banner?: ImageFile,
    avatar?: ImageFile,
  ): Promise<Venue> => {
    const formData = new FormData();
    formData.append("Name", venue.name);
    formData.append("About", venue.about);
    formData.append("Latitude", String(venue.latitude));
    formData.append("Longitude", String(venue.longitude));
    if (banner) formData.append("Banner", banner as any);
    if (avatar) formData.append("Avatar", avatar as any);
    const { data } = await apiClient.put<Venue>("/organization/venue", formData);
    return data;
  },
};

export default venueApi;
