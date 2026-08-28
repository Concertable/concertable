import { paymentClient } from "../../../lib/paymentClient";
import type { PayoutAccountStatus, PaymentMethod } from "../types";

const BASE = "/stripeaccount";

const stripeAccountApi = {
  getOnboardingLink: async (): Promise<string> => {
    const { data } = await paymentClient.get<string>(`${BASE}/onboarding-link`);
    return data;
  },

  getAccountStatus: async (): Promise<PayoutAccountStatus> => {
    const { data } = await paymentClient.get<PayoutAccountStatus>(
      `${BASE}/account-status`,
    );
    return data;
  },

  createSetupIntent: async (): Promise<string> => {
    const { data } = await paymentClient.post<string>(`${BASE}/setup-intent`);
    return data;
  },

  getPaymentMethod: async (): Promise<PaymentMethod | undefined> => {
    const { data } = await paymentClient.get<PaymentMethod | undefined>(
      `${BASE}/payment-method`,
    );
    return data;
  },
};

export default stripeAccountApi;
