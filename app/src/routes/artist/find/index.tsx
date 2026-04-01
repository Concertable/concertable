import { createFileRoute } from "@tanstack/react-router";
import FindVenuePage from "@/pages/artist/find/FindVenuePage";
import { searchSchema } from "@/lib/searchParams";

export const Route = createFileRoute("/artist/find/")({
  component: FindVenuePage,
  validateSearch: searchSchema("venue"),
});
