import { createFileRoute } from "@tanstack/react-router";
import FindVenuePage from "@/pages/artist/find/FindVenuePage";

export const Route = createFileRoute("/artist/find/")({
  component: FindVenuePage,
});
