import { createFileRoute } from "@tanstack/react-router";
import { CreateArtistPage } from "@/features/artists";

export const Route = createFileRoute("/artist/create")({
  component: CreateArtistPage,
});
