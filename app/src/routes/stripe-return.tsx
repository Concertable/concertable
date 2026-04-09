import { createFileRoute } from "@tanstack/react-router";
import StripeReturnPage from "@/pages/stripe/StripeReturnPage";

export const Route = createFileRoute("/stripe-return")({
  component: StripeReturnPage,
});
