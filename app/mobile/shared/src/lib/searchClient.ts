import { searchClient } from "@concertable/shared/lib/searchClient";
import Config from "./config";
import { configureMobileClient } from "./configureMobileClient";

configureMobileClient(searchClient, `${Config.searchApiUrl}/api`);

export { searchClient };
