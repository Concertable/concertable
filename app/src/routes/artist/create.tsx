import { createFileRoute } from "@tanstack/react-router";
import CreateArtistPage from "@/pages/artist/CreateArtistPage";

export const Route = createFileRoute("/artist/create")({
  component: CreateArtistPage,
});
