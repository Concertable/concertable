import { createFileRoute } from "@tanstack/react-router";
import FindArtistPage from "@/pages/venue/find/FindArtistPage";

export const Route = createFileRoute("/venue/find/")({
  component: FindArtistPage,
});
