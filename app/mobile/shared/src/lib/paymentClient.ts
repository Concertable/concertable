import { paymentClient } from "@concertable/shared/lib/paymentClient";
import Config from "./config";
import { configureMobileClient } from "./configureMobileClient";

configureMobileClient(paymentClient, `${Config.paymentApiUrl}/api`);

export { paymentClient };
