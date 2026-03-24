import { createFileRoute, Outlet } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";

export const Route = createFileRoute("/artist")({
  beforeLoad: () => requireRole("ArtistManager"),
  component: Outlet,
});
