import { createFileRoute } from "@tanstack/react-router";
import { MyArtistPage } from "@/features/artists";

export const Route = createFileRoute("/_artist/my/")({
  component: MyArtistPage,
});
