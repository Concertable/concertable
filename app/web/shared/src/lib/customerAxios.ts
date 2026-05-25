import { AxiosError } from "axios";
import { userManager } from "@/features/auth";
import customerApi, { configureCustomerApi } from "@concertable/shared/lib/customerAxiosClient";

configureCustomerApi(import.meta.env.VITE_CUSTOMER_API_URL);

customerApi.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) config.headers.Authorization = `Bearer ${user.access_token}`;
  return config;
});

customerApi.interceptors.response.use(
  (res) => res,
  (error: AxiosError) => Promise.reject(error),
);

export default customerApi;
