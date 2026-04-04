import { createFileRoute } from "@tanstack/react-router";
import TicketsPage from "@/pages/customer/profile/tickets/TicketsPage";

export const Route = createFileRoute("/_customer/settings/tickets/")({
  component: TicketsPage,
});
