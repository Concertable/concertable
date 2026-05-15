import api from "../../../lib/axiosClient";
import type { Venue } from "../types";
import type { ImageFile } from "../../../types/image";

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
    banner?: ImageFile,
    avatar?: ImageFile,
  ): Promise<Venue> => {
    const formData = new FormData();
    formData.append("Name", venue.name);
    formData.append("About", venue.about);
    formData.append("Latitude", String(venue.latitude));
    formData.append("Longitude", String(venue.longitude));
    if (banner) {
      formData.append("Banner.Url", venue.bannerUrl);
      formData.append("Banner.File", banner as any);
    }
    if (avatar) formData.append("Avatar", avatar as any);
    const { data } = await api.put<Venue>(`/venue/${venue.id}`, formData);
    return data;
  },
};

export default venueApi;
