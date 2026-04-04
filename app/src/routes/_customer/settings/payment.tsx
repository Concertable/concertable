import { createFileRoute } from "@tanstack/react-router";
import PaymentPage from "@/pages/customer/profile/PaymentPage";

export const Route = createFileRoute("/_customer/settings/payment")({
  component: PaymentPage,
});
