import api from "@/lib/axios";
import type { Artist } from "../types";

const artistApi = {
  getArtist: async (id: number): Promise<Artist> => {
    const { data } = await api.get<Artist>(`/artist/${id}`);
    return data;
  },

  getMyArtist: async (): Promise<Artist> => {
    const { data } = await api.get<Artist>("/artist/user");
    return data;
  },

  updateArtist: async (
    artist: Artist,
    banner?: File,
    avatar?: File,
  ): Promise<Artist> => {
    const formData = new FormData();
    formData.append("artist", JSON.stringify(artist));
    if (banner) formData.append("Banner", banner);
    if (avatar) formData.append("Avatar", avatar);
    const { data } = await api.put<Artist>(`/artist/${artist.id}`, formData);
    return data;
  },
};

export default artistApi;
