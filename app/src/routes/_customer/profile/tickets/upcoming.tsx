import { createFileRoute } from "@tanstack/react-router";
import UpcomingTicketsPage from "@/pages/customer/profile/tickets/UpcomingTicketsPage";

export const Route = createFileRoute("/_customer/profile/tickets/upcoming")({
  component: UpcomingTicketsPage,
});
