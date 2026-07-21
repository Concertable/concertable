import { searchClient } from "@concertable/shared/lib/searchClient";
import { configureWebClient } from "./configureWebClient";

configureWebClient(searchClient, import.meta.env.VITE_SEARCH_API_URL);

export { searchClient };
