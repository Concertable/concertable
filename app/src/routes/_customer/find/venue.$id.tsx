import { createFileRoute } from "@tanstack/react-router";
import VenueDetailsPage from "@/pages/customer/find/VenueDetailsPage";

export const Route = createFileRoute("/_customer/find/venue/$id")({
  component: VenueDetailsPage,
});
