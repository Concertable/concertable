import api from "@/lib/axios";
import type { OpportunityApplication } from "@/types/application";

export async function applyToOpportunity(opportunityId: number): Promise<OpportunityApplication> {
  const { data } = await api.post<OpportunityApplication>(`/opportunityapplication/${opportunityId}`);
  return data;
}

export async function getApplicationsByOpportunityId(opportunityId: number): Promise<OpportunityApplication[]> {
  const { data } = await api.get<OpportunityApplication[]>(`/opportunityapplication/opportunity/${opportunityId}`);
  return data;
}

export async function getApplicationById(applicationId: number): Promise<OpportunityApplication> {
  const { data } = await api.get<OpportunityApplication>(`/opportunityapplication/${applicationId}`);
  return data;
}

export async function acceptApplication(applicationId: number): Promise<void> {
  await api.post(`/opportunityapplication/accept/${applicationId}`);
}

export async function canAcceptApplication(applicationId: number): Promise<boolean> {
  const { data } = await api.get<boolean>(`/opportunityapplication/can-accept/${applicationId}`);
  return data;
}
