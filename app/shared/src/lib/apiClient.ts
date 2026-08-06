import axios, {
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
  client.getOptional = <T = unknown, D = unknown>(
    url: string,
    config?: AxiosRequestConfig<D>,
  ) =>
    client.get<T | null, AxiosResponse<T | null, D>, D>(url, {
      ...config,
      notFoundAsNull: true,
    } as AxiosRequestConfig<D>);
  return client;
}

export const apiClient = createApiClient();
