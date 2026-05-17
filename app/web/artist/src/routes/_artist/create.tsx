import { createFileRoute } from "@tanstack/react-router";
import { CreateArtistPage } from "@/features/artists";

export const Route = createFileRoute("/_artist/create")({
  component: CreateArtistPage,
});
