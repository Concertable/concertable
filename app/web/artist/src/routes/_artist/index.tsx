import { createFileRoute } from "@tanstack/react-router";
import { ArtistDashboardPage } from "@/features/artists";

export const Route = createFileRoute("/_artist/")({
  component: ArtistDashboardPage,
});
