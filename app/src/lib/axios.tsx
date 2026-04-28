import axios, { AxiosError } from "axios";
import qs from "qs";
import { toast } from "sonner";
import { useAuthStore } from "@/store/useAuthStore";

type ProblemDetails = {
  title?: string;
  detail?: string;
  errors?: string[];
};

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
  paramsSerializer: (params) =>
    qs.stringify(params, {
      arrayFormat: "comma",
      encode: false,
      skipNulls: true,
    }),
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError<ProblemDetails>) => {
    const { config, response } = error;

    if (response?.status === 401 && config?.url !== "/auth/refresh") {
      await useAuthStore.getState().refresh();
      const token = useAuthStore.getState().accessToken;
      if (config && token) {
        config.headers.Authorization = `Bearer ${token}`;
        return api(config);
      }
    }

    if (response) {
      const { title, detail, errors } = response.data ?? {};
      if (errors?.length) {
        toast.error(title ?? "Error", {
          description: (
            <ul className="list-disc space-y-1 pl-4">
              {errors.map((e, i) => (
                <li key={i}>{e}</li>
              ))}
            </ul>
          ),
        });
      } else {
        toast.error(detail ?? "Something went wrong");
      }
    }

    return Promise.reject(error);
  },
);

export default api;
