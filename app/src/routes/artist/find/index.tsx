import { createFileRoute } from "@tanstack/react-router";
import FindVenuePage from "@/pages/artist/find/FindVenuePage";
import { validateSearchFilters } from "@/lib/searchParams";

export const Route = createFileRoute("/artist/find/")({
  component: FindVenuePage,
  validateSearch: validateSearchFilters("venue"),
});
