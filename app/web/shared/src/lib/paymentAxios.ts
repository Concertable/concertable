import { AxiosError } from "axios";
import { userManager } from "@/features/auth";
import { TENANT_HEADER, useActiveTenantStore } from "@/features/tenant";
import paymentApi, { configurePaymentApi } from "@concertable/shared/lib/paymentAxiosClient";

configurePaymentApi(import.meta.env.VITE_PAYMENT_API_URL);

paymentApi.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) config.headers.Authorization = `Bearer ${user.access_token}`;
  // B2B apps point this client at their own payout proxy, which resolves the active tenant from this header;
  // Customer points it at Payment, which ignores it. Only sent once a tenant is selected (Phase 6 switcher).
  const tenantId = useActiveTenantStore.getState().activeTenantId;
  if (tenantId) config.headers[TENANT_HEADER] = tenantId;
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
