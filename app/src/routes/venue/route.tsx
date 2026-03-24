import { createFileRoute, Outlet } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";

export const Route = createFileRoute("/venue")({
  beforeLoad: () => requireRole("VenueManager"),
  component: Outlet,
});
