import { createFileRoute } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";
import ArtistLayout from "@/layouts/ArtistLayout";

export const Route = createFileRoute("/artist")({
  beforeLoad: () => requireRole("ArtistManager"),
  component: ArtistLayout,
});
