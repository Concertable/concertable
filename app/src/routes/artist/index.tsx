import { createFileRoute } from "@tanstack/react-router";
import ArtistDashboardPage from "@/pages/artist/ArtistDashboardPage";

export const Route = createFileRoute("/artist/")({
  component: ArtistDashboardPage,
});
