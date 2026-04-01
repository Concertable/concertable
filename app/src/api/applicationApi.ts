import api from "@/lib/axios";
import type { ConcertApplication } from "@/types/application";

export async function applyToOpportunity(opportunityId: number): Promise<ConcertApplication> {
  const { data } = await api.post<ConcertApplication>(`/concertapplication/${opportunityId}`);
  return data;
}

export async function getApplicationsByOpportunityId(opportunityId: number): Promise<ConcertApplication[]> {
  const { data } = await api.get<ConcertApplication[]>(`/concertapplication/opportunity/${opportunityId}`);
  return data;
}

export async function getApplicationById(applicationId: number): Promise<ConcertApplication> {
  const { data } = await api.get<ConcertApplication>(`/concertapplication/${applicationId}`);
  return data;
}

export async function acceptApplication(applicationId: number): Promise<void> {
  await api.post(`/concertapplication/accept/${applicationId}`);
}

export async function canAcceptApplication(applicationId: number): Promise<boolean> {
  const { data } = await api.get<boolean>(`/concertapplication/can-accept/${applicationId}`);
  return data;
}
