import { createFileRoute } from "@tanstack/react-router";
import StripeRefreshPage from "@/pages/stripe/StripeRefreshPage";

export const Route = createFileRoute("/stripe-refresh")({
  component: StripeRefreshPage,
});
