import { createFileRoute } from "@tanstack/react-router";
import { ApplicationCheckoutPage } from "@/features/concerts";

export const Route = createFileRoute("/_venue/applications/$applicationId/checkout")({
  params: {
    parse: (params) => ({ applicationId: Number(params.applicationId) }),
    stringify: (params) => ({ applicationId: String(params.applicationId) }),
  },
  component: ApplicationCheckoutPage,
});
