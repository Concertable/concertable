import api from "@/lib/axios";
import type { ConcertApplication } from "@/types/application";

export async function applyToOpportunity(opportunityId: number): Promise<ConcertApplication> {
  const { data } = await api.post<ConcertApplication>(`/concertapplication/${opportunityId}`);
  return data;
}
