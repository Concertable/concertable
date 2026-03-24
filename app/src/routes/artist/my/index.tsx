import { createFileRoute } from "@tanstack/react-router";
import MyArtistPage from "@/pages/artist/my/MyArtistPage";

export const Route = createFileRoute("/artist/my/")({
  component: MyArtistPage,
});
