import axios from "axios";
import qs from "qs";

export const searchClient = axios.create({
  // paired with Search's comma binder — repeated/bracket keys would break multi-genre search
  paramsSerializer: (params) =>
    qs.stringify(params, { arrayFormat: "comma", encode: false, skipNulls: true }),
});
