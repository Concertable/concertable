import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/concert/checkout/$id")({
  component: () => <div>Concert Checkout</div>,
});
