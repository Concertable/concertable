import { paymentClient } from "@concertable/shared/lib/paymentClient";
import { configureWebClient } from "./configureWebClient";

configureWebClient(paymentClient, import.meta.env.VITE_PAYMENT_API_URL);

export { paymentClient };
