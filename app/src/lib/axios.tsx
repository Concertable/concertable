import axios, { AxiosError } from "axios";
import qs from "qs";
import { toast } from "sonner";
import { userManager } from "@/lib/oidcConfig";

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

api.interceptors.request.use(async (config) => {
  const user = await userManager.getUser();
  if (user?.access_token) config.headers.Authorization = `Bearer ${user.access_token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError<ProblemDetails>) => {
    const { response } = error;

    if (response?.status === 401) {
      await userManager.signinRedirect();
      return Promise.reject(error);
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
