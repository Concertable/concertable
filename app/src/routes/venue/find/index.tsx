import { createFileRoute } from "@tanstack/react-router";
import FindArtistPage from "@/pages/venue/find/FindArtistPage";
import { validateSearchFilters } from "@/lib/searchParams";

export const Route = createFileRoute("/venue/find/")({
  component: FindArtistPage,
  validateSearch: validateSearchFilters("artist"),
});
