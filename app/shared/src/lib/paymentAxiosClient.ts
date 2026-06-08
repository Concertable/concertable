import axios from "axios";
import qs from "qs";

const paymentApi = axios.create({
  paramsSerializer: (params) =>
    qs.stringify(params, {
      arrayFormat: "comma",
      encode: false,
      skipNulls: true,
    }),
});

export function configurePaymentApi(baseURL: string) {
  paymentApi.defaults.baseURL = baseURL;
}

export default paymentApi;
