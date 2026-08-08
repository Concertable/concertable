import type { AxiosInstance } from "axios";

export function configureClient(instance: AxiosInstance, baseURL: string) {
  instance.defaults.baseURL = baseURL;
  const builder = {
    withAuth(
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
        async (error) => {
          if (error.response?.status === 401) await onUnauthorized();
          return Promise.reject(error);
        },
      );
      return builder;
    },
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
