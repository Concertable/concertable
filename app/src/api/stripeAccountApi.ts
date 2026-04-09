import api from "@/lib/axios";

export async function getOnboardingLink(): Promise<string> {
  const { data } = await api.get<string>("/stripeaccount/onboarding-link");
  return data;
}

export async function isVerified(): Promise<boolean> {
  const { data } = await api.get<boolean>("/stripeaccount/verified");
  return data;
}

export async function createSetupIntent(): Promise<string> {
  const { data } = await api.post<string>("/stripeaccount/setup-intent");
  return data;
}
