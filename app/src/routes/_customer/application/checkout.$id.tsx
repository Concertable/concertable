import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/application/checkout/$id")({
  component: () => <div>Application Checkout</div>,
});
