import { createFileRoute } from "@tanstack/react-router";
import { VenueHomePage } from "@/features/venues";

export const Route = createFileRoute("/")({
  component: VenueHomePage,
});
