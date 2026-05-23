import { AxiosError } from "axios";
import { userManager } from "@/features/auth";
import searchApi, { configureSearchApi } from "@concertable/shared/lib/searchAxiosClient";

configureSearchApi(import.meta.env.VITE_SEARCH_API_URL);

searchApi.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) config.headers.Authorization = `Bearer ${user.access_token}`;
  return config;
});

searchApi.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    if (error.response?.status === 401) await userManager.removeUser();
    return Promise.reject(error);
  },
);

export default searchApi;
