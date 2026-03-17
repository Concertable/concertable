import { createFileRoute } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";
import VenueLayout from "@/layouts/VenueLayout";

export const Route = createFileRoute("/venue")({
  beforeLoad: () => requireRole("VenueManager"),
  component: VenueLayout,
});
