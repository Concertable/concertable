import { apiClient } from "../../../lib/apiClient";
import type { Venue } from "../types";

const BASE = "/venue";

const venueApi = {
  getVenue: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`${BASE}/${id}`);
    return data;
  },

  getVenueById: async (id: number): Promise<Venue> => {
    const { data } = await apiClient.get<Venue>(`${BASE}/${id}`);
    return data;
  },
};

export default venueApi;
