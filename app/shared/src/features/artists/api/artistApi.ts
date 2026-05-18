import api from "../../../lib/axiosClient";
import type { Artist } from "../types";
import type { ImageFile } from "../../../types/image";
import type { Genre } from "../../../types/common";

export interface CreateArtist {
  name: string;
  about: string;
  latitude: number;
  longitude: number;
  genres: Genre[];
  banner: File;
  avatar: File;
}

const artistApi = {
  getArtist: async (id: number): Promise<Artist> => {
    const { data } = await api.get<Artist>(`/artist/${id}`);
    return data;
  },

  getMyArtist: async (): Promise<Artist | null> => {
    const { data, status } = await api.get<Artist>("/artist/user");
    return status === 204 ? null : data;
  },

  createArtist: async (input: CreateArtist): Promise<Artist> => {
    const formData = new FormData();
    formData.append("Name", input.name);
    formData.append("About", input.about);
    formData.append("Latitude", String(input.latitude));
    formData.append("Longitude", String(input.longitude));
    formData.append("Banner", input.banner);
    formData.append("Avatar", input.avatar);
    input.genres.forEach((g, i) => {
      formData.append(`Genres[${i}].Id`, String(g.id));
      formData.append(`Genres[${i}].Name`, g.name);
    });
    const { data } = await api.post<Artist>("/artist", formData);
    return data;
  },

  updateArtist: async (
    artist: Artist,
    banner?: ImageFile,
    avatar?: ImageFile,
  ): Promise<Artist> => {
    const formData = new FormData();
    formData.append("artist", JSON.stringify(artist));
    if (banner) formData.append("Banner", banner as any);
    if (avatar) formData.append("Avatar", avatar as any);
    const { data } = await api.put<Artist>(`/artist/${artist.id}`, formData);
    return data;
  },
};

export default artistApi;
