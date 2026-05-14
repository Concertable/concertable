import api from "@/lib/axios";
import type { Venue } from "../types";

const venueApi = {
  getVenue: async (id: number): Promise<Venue> => {
    const { data } = await api.get<Venue>(`/venue/${id}`);
    return data;
  },

  getMyVenue: async (): Promise<Venue> => {
    const { data } = await api.get<Venue>("/venue/user");
    return data;
  },

  updateVenue: async (
    venue: Venue,
    banner?: File,
    avatar?: File,
  ): Promise<Venue> => {
    const formData = new FormData();
    formData.append("Name", venue.name);
    formData.append("About", venue.about);
    formData.append("Latitude", String(venue.latitude));
    formData.append("Longitude", String(venue.longitude));
    formData.append("Approved", String(venue.approved));
    if (banner) {
      formData.append("Banner.Url", venue.bannerUrl);
      formData.append("Banner.File", banner);
    }
    if (avatar) formData.append("Avatar", avatar);
    const { data } = await api.put<Venue>(`/venue/${venue.id}`, formData);
    return data;
  },
};

export default venueApi;
