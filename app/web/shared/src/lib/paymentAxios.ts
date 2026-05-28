import { AxiosError } from "axios";
import { userManager } from "@/features/auth";
import paymentApi, { configurePaymentApi } from "@concertable/shared/lib/paymentAxiosClient";

configurePaymentApi(import.meta.env.VITE_PAYMENT_API_URL);

paymentApi.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) config.headers.Authorization = `Bearer ${user.access_token}`;
  return config;
});

paymentApi.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    if (error.response?.status === 401) await userManager.removeUser();
    return Promise.reject(error);
  },
);

export default paymentApi;
