import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/payment")({
  component: () => <div>Payment Details</div>,
});
