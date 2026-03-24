import { createFileRoute } from "@tanstack/react-router";
import ArtistDetailsPage from "@/pages/venue/find/ArtistDetailsPage";

export const Route = createFileRoute("/venue/find/artist/$id")({
  component: ArtistDetailsPage,
});
