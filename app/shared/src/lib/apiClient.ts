import axios, {
  isAxiosError,
  type AxiosInstance,
  type CreateAxiosDefaults,
  type AxiosRequestConfig,
  type AxiosResponse,
} from "axios";
import { isApiError } from "./apiError";

export type ApiClient = AxiosInstance & {
  getOptional<T = unknown, D = unknown>(
    url: string,
    config?: AxiosRequestConfig<D>,
  ): Promise<AxiosResponse<T | null, D>>;
};

export function createApiClient(options?: CreateAxiosDefaults): ApiClient {
  const client = axios.create(options) as ApiClient;
  client.getOptional = async <T = unknown, D = unknown>(
    url: string,
    config?: AxiosRequestConfig<D>,
  ): Promise<AxiosResponse<T | null, D>> => {
    try {
      return await client.get<T, AxiosResponse<T, D>, D>(url, config);
    } catch (error) {
      const requestError = isApiError(error) ? error.cause : error;
      if (isAxiosError(requestError) && requestError.response?.status === 404)
        return { ...requestError.response, data: null };
      throw error;
    }
  };
  return client;
}

export const apiClient = createApiClient();
