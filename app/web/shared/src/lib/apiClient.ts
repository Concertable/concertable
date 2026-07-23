import { apiClient } from "@concertable/shared/lib/apiClient";
import { configureWebClient } from "./configureWebClient";

configureWebClient(apiClient, import.meta.env.VITE_API_URL);

export { apiClient };
