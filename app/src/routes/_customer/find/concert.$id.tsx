import { createFileRoute } from "@tanstack/react-router";
import ConcertDetailsPage from "@/pages/customer/find/ConcertDetailsPage";

export const Route = createFileRoute("/_customer/find/concert/$id")({
  component: ConcertDetailsPage,
});
