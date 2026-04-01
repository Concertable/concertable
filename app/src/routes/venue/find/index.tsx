import { createFileRoute } from "@tanstack/react-router";
import FindArtistPage from "@/pages/venue/find/FindArtistPage";
import { searchSchema } from "@/lib/searchParams";

export const Route = createFileRoute("/venue/find/")({
  component: FindArtistPage,
  validateSearch: searchSchema("artist"),
});
