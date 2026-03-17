import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/find/venue/$id")({
  component: () => <div>Venue Details</div>,
});
