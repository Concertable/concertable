import { createFileRoute } from "@tanstack/react-router";
import VenueDetailsPage from "@/pages/artist/find/VenueDetailsPage";

export const Route = createFileRoute("/artist/find/venue/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: VenueDetailsPage,
});
