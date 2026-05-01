import { createFileRoute } from "@tanstack/react-router";
import { FindArtistPage, SearchSchema } from "@/features/search";

export const Route = createFileRoute("/venue/find/")({
  component: FindArtistPage,
  validateSearch: SearchSchema(),
});
