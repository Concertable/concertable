import api from "@/lib/axios";
import type { Artist } from "@/types/artist";

export async function getArtist(id: number): Promise<Artist> {
  const { data } = await api.get<Artist>(`/artist/${id}`);
  return data;
}

export async function getMyArtist(): Promise<Artist> {
  const { data } = await api.get<Artist>("/artist/user");
  return data;
}

export async function updateArtist(
  artist: Artist,
  image?: File,
): Promise<Artist> {
  const formData = new FormData();
  formData.append("artist", JSON.stringify(artist));
  if (image) formData.append("image", image);
  const { data } = await api.put<Artist>(`/artist/${artist.id}`, formData);
  return data;
}
