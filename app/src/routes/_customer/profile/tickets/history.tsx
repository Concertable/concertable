import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/tickets/history")({
  component: () => <div>Ticket History</div>,
});
