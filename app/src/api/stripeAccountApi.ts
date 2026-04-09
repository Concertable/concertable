import api from "@/lib/axios";

const stripeAccountApi = {
  getOnboardingLink: async (): Promise<string> => {
    const { data } = await api.get<string>("/stripeaccount/onboarding-link");
    return data;
  },

  isVerified: async (): Promise<boolean> => {
    const { data } = await api.get<boolean>("/stripeaccount/verified");
    return data;
  },

  createSetupIntent: async (): Promise<string> => {
    const { data } = await api.post<string>("/stripeaccount/setup-intent");
    return data;
  },
};

export default stripeAccountApi;
