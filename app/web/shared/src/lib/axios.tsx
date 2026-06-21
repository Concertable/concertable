import { AxiosError } from "axios";
import { userManager } from "@/features/auth";
import api, { configureApi } from "@concertable/shared/lib/axiosClient";

configureApi(import.meta.env.VITE_API_URL);

api.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) config.headers.Authorization = `Bearer ${user.access_token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    if (error.response?.status === 401) await userManager.removeUser();
    return Promise.reject(error);
  },
);

export default api;
