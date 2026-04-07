import api from "@/lib/axios";
import type { Venue } from "@/types/venue";

export async function getVenue(id: number): Promise<Venue> {
  const { data } = await api.get<Venue>(`/venue/${id}`);
  return data;
}

export async function getMyVenue(): Promise<Venue> {
  const { data } = await api.get<Venue>("/venue/user");
  return data;
}

export async function updateVenue(
  venue: Venue,
  banner?: File,
  avatar?: File,
): Promise<Venue> {
  const formData = new FormData();
  formData.append("venue", JSON.stringify(venue));
  if (banner) formData.append("Banner", banner);
  if (avatar) formData.append("Avatar", avatar);
  const { data } = await api.put<Venue>(`/venue/${venue.id}`, formData);
  return data;
}
