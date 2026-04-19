import { createFileRoute } from "@tanstack/react-router";
import ApplicationCheckoutPage from "@/pages/customer/application/ApplicationCheckoutPage";

export const Route = createFileRoute(
  "/artist/application/checkout/$applicationId",
)({
  params: {
    parse: (params) => ({ applicationId: Number(params.applicationId) }),
    stringify: (params) => ({ applicationId: String(params.applicationId) }),
  },
  component: ApplicationCheckoutPage,
});
