import { createFileRoute } from "@tanstack/react-router";
import FindArtistPage from "@/pages/venue/find/FindArtistPage";
import { SearchSchema } from "@/schemas/searchSchema";

export const Route = createFileRoute("/venue/find/")({
  component: FindArtistPage,
  validateSearch: SearchSchema("artist"),
});
