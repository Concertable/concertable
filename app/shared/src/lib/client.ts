import { isAxiosError, type AxiosInstance } from "axios";
import { ApiError, type ProblemDetails } from "./apiError";

export function configureClient(instance: AxiosInstance, baseURL: string) {
  instance.defaults.baseURL = baseURL;
  const builder = {
    withTenant(getTenantId: () => string | undefined, headerName: string) {
      instance.interceptors.request.use((config) => {
        const tenantId = getTenantId();
        if (tenantId) config.headers[headerName] = tenantId;
        return config;
      });
      return builder;
    },
  };
  return builder;
}

export function attachAuth(
  instance: AxiosInstance,
  getToken: () => Promise<string | null> | string | null,
  onUnauthorized: () => void | Promise<void>,
) {
  instance.interceptors.request.use(async (config) => {
    const token = await getToken();
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
  });
  instance.interceptors.response.use(
    (res) => res,
    async (error: unknown) => {
      if (!isAxiosError(error)) return Promise.reject(error);

      const status = error.response?.status ?? null;
      if (status === 401) await onUnauthorized();
      const responseData = error.response?.data;
      const details =
        typeof responseData === "object" && responseData !== null
          ? (responseData as ProblemDetails)
          : {};
      return Promise.reject(
        new ApiError(
          status,
          details,
          error.config?.method,
          error.config?.url,
          error,
        ),
      );
    },
  );
}
