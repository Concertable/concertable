import api from "@/lib/axios";

export interface PaymentMethod {
  brand: string;
  last4: string;
  expMonth: number;
  expYear: number;
}

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

  getPaymentMethod: async (): Promise<PaymentMethod | null> => {
    const { data } = await api.get<PaymentMethod | null>(
      "/stripeaccount/payment-method",
    );
    return data;
  },
};

export default stripeAccountApi;
