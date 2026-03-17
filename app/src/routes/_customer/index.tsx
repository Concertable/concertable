import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/")({
  component: () => <div>Customer Dashboard</div>,
});
