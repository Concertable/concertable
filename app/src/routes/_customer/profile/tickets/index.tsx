import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/tickets/")({
  component: () => <div>My Tickets</div>,
});
