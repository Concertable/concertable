import { createFileRoute } from "@tanstack/react-router";
import TicketHistoryPage from "@/pages/customer/profile/tickets/TicketHistoryPage";

export const Route = createFileRoute("/_customer/settings/tickets/history")({
  component: TicketHistoryPage,
});
