import { createFileRoute } from "@tanstack/react-router";
import { FindVenuePage, SearchSchema } from "@/features/search";

export const Route = createFileRoute("/artist/find/")({
  component: FindVenuePage,
  validateSearch: SearchSchema(),
});
