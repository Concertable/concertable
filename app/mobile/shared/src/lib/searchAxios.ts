import searchApi, { configureSearchApi } from "@concertable/shared/lib/searchAxiosClient";
import { useAuthStore } from "@concertable/shared/features/auth";
import { tokenStorage } from "../auth/tokenStorage";
import Config from "./config";

configureSearchApi(`${Config.searchApiUrl}/api`);

searchApi.interceptors.request.use(async (config) => {
  const token = await tokenStorage.getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

searchApi.interceptors.response.use(
  (r) => r,
  async (error) => {
    if (error.response?.status === 401) {
      await tokenStorage.clear();
      useAuthStore.getState().setUser(null);
    }
    return Promise.reject(error);
  },
);

export default searchApi;
