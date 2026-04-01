import axios, { AxiosError } from "axios";
import qs from "qs";
import { useAuthStore } from "@/store/useAuthStore";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
  paramsSerializer: (params) => qs.stringify(params, { arrayFormat: "comma", encode: false, skipNulls: true }),
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  console.log("[axios] request:", config.url, config.params);
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error: unknown) => {
    if (
      error instanceof AxiosError &&
      error.response?.status === 401 &&
      error.config?.url !== "/auth/refresh"
    ) {
      await useAuthStore.getState().refresh();
      const token = useAuthStore.getState().accessToken;
      if (error.config && token) {
        error.config.headers.Authorization = `Bearer ${token}`;
        return api(error.config);
      }
    }

    return Promise.reject(error instanceof Error ? error : new Error(String(error)));
  }
);

export default api;
