import qs from "qs";
import { createApiClient } from "./apiClient";

export const searchClient = createApiClient({
  // paired with Search's comma binder — repeated/bracket keys would break multi-genre search
  paramsSerializer: (params) =>
    qs.stringify(params, { arrayFormat: "comma", encode: false, skipNulls: true }),
});
