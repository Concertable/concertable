import { createFileRoute } from "@tanstack/react-router";
import { ApplicationCheckoutPage } from "@/features/concerts";

export const Route = createFileRoute(
  "/artist/application/checkout/$applicationId",
)({
  params: {
    parse: (params) => ({ applicationId: Number(params.applicationId) }),
    stringify: (params) => ({ applicationId: String(params.applicationId) }),
  },
  component: ApplicationCheckoutPage,
});
