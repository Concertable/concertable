import api from "@/lib/axios";

export type PayoutAccountStatus = "NotVerified" | "Pending" | "Verified";

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

  getAccountStatus: async (): Promise<StripeAccountStatus> => {
    const { data } = await api.get<StripeAccountStatus>(
      "/stripeaccount/account-status",
    );
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
