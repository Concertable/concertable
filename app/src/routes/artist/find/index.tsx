import { createFileRoute } from "@tanstack/react-router";
import FindVenuePage from "@/pages/artist/find/FindVenuePage";
import { SearchSchema } from "@/schemas/searchSchema";

export const Route = createFileRoute("/artist/find/")({
  component: FindVenuePage,
  validateSearch: SearchSchema("venue"),
});
