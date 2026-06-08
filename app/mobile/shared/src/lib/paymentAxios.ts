import paymentApi, { configurePaymentApi } from "@concertable/shared/lib/paymentAxiosClient";
import { useAuthStore } from "@concertable/shared/features/auth";
import { tokenStorage } from "../auth/tokenStorage";
import Config from "./config";

configurePaymentApi(`${Config.paymentApiUrl}/api`);

paymentApi.interceptors.request.use(async (config) => {
  const token = await tokenStorage.getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

paymentApi.interceptors.response.use(
  (r) => r,
  async (error) => {
    if (error.response?.status === 401) {
      await tokenStorage.clear();
      useAuthStore.getState().setUser(null);
    }
    return Promise.reject(error);
  },
);

export default paymentApi;
