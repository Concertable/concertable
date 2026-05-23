import axios from "axios";
import qs from "qs";

const searchApi = axios.create({
  paramsSerializer: (params) =>
    qs.stringify(params, {
      arrayFormat: "comma",
      encode: false,
      skipNulls: true,
    }),
});

export function configureSearchApi(baseURL: string) {
  searchApi.defaults.baseURL = baseURL;
}

export default searchApi;
