import axios, {
  isAxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
} from "axios";

export type ApiClient = AxiosInstance & {
  getOptional<T = unknown, D = unknown>(
    url: string,
    config?: AxiosRequestConfig<D>,
  ): Promise<AxiosResponse<T | null, D>>;
};

export function createApiClient(): ApiClient {
  const client = axios.create() as ApiClient;
  client.getOptional = async <T = unknown, D = unknown>(
    url: string,
    config?: AxiosRequestConfig<D>,
  ): Promise<AxiosResponse<T | null, D>> => {
    try {
      return await client.get<T, AxiosResponse<T, D>, D>(url, config);
    } catch (error) {
      if (isAxiosError(error) && error.response?.status === 404)
        return { ...error.response, data: null };
      throw error;
    }
  };
  return client;
}

export const apiClient = createApiClient();
