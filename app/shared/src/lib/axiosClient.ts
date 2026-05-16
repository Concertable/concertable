import axios from "axios";
import qs from "qs";

const api = axios.create({
  paramsSerializer: (params) =>
    qs.stringify(params, {
      arrayFormat: "comma",
      encode: false,
      skipNulls: true,
    }),
});

export function configureApi(baseURL: string) {
  api.defaults.baseURL = baseURL;
}

export default api;
