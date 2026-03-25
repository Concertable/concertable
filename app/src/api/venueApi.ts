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

export async function updateVenue(venue: Venue, image?: File): Promise<Venue> {
  const formData = new FormData();
  formData.append("venue", JSON.stringify(venue));
  if (image) formData.append("image", image);
  const { data } = await api.put<Venue>(`/venue/${venue.id}`, formData);
  return data;
}
