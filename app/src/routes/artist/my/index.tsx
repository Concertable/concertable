import { createFileRoute } from "@tanstack/react-router";
import { MyArtistPage } from "@/features/artists";

export const Route = createFileRoute("/artist/my/")({
  component: MyArtistPage,
});
