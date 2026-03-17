import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/tickets/upcoming")({
  component: () => <div>Upcoming Tickets</div>,
});
