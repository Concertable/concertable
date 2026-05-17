import { createFileRoute } from "@tanstack/react-router";
import { ArtistHomePage } from "@/features/artists";

export const Route = createFileRoute("/")({
  component: ArtistHomePage,
});
