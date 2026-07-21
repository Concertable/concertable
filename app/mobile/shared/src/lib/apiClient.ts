import { apiClient } from "@concertable/shared/lib/apiClient";
import Config from "./config";
import { configureMobileClient } from "./configureMobileClient";

configureMobileClient(apiClient, `${Config.apiUrl}/api`);

export { apiClient };
