import { createFileRoute } from "@tanstack/react-router";
import VenueDetailsPage from "@/pages/artist/find/VenueDetailsPage";

export const Route = createFileRoute("/artist/find/venue/$id")({
  component: VenueDetailsPage,
});
