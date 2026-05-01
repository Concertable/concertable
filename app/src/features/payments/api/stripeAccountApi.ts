import api from "@/lib/axios";
import type { PayoutAccountStatus, PaymentMethod } from "../types";

const stripeAccountApi = {
  getOnboardingLink: async (): Promise<string> => {
    const { data } = await api.get<string>("/stripeaccount/onboarding-link");
    return data;
  },

  getAccountStatus: async (): Promise<PayoutAccountStatus> => {
    const { data } = await api.get<PayoutAccountStatus>(
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
