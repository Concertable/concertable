import { apiClient } from "../../../lib/apiClient";
import type { Artist } from "../types";

const BASE = "/artist";

const artistApi = {
  getArtist: async (id: number): Promise<Artist> => {
    const { data } = await apiClient.get<Artist>(`${BASE}/${id}`);
    return data;
  },

  getArtistById: async (id: number): Promise<Artist> => {
    const { data } = await apiClient.get<Artist>(`${BASE}/${id}`);
    return data;
  },
};

export default artistApi;
