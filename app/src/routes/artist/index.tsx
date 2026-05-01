import { createFileRoute } from "@tanstack/react-router";
import { ArtistDashboardPage } from "@/features/artists";

export const Route = createFileRoute("/artist/")({
  component: ArtistDashboardPage,
});
