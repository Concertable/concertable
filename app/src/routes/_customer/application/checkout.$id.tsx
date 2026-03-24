import { createFileRoute } from "@tanstack/react-router";
import ApplicationCheckoutPage from "@/pages/customer/application/ApplicationCheckoutPage";

export const Route = createFileRoute("/_customer/application/checkout/$id")({
  component: ApplicationCheckoutPage,
});
