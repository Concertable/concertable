import { paymentClient } from "../../../lib/paymentClient";
import type { PayoutAccountStatus, PaymentMethod } from "../types";

const stripeAccountApi = {
  getOnboardingLink: async (): Promise<string> => {
    const { data } = await paymentClient.get<string>("/stripeaccount/onboarding-link");
    return data;
  },

  getAccountStatus: async (): Promise<PayoutAccountStatus> => {
    const { data } = await paymentClient.get<PayoutAccountStatus>(
      "/stripeaccount/account-status",
    );
    return data;
  },

  createSetupIntent: async (): Promise<string> => {
    const { data } = await paymentClient.post<string>("/stripeaccount/setup-intent");
    return data;
  },

  getPaymentMethod: async (): Promise<PaymentMethod | undefined> => {
    const { data } = await paymentClient.get<PaymentMethod | undefined>(
      "/stripeaccount/payment-method",
    );
    return data;
  },
};

export default stripeAccountApi;
