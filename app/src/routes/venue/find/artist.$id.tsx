import { createFileRoute } from "@tanstack/react-router";
import ArtistDetailsPage from "@/pages/venue/find/ArtistDetailsPage";

export const Route = createFileRoute("/venue/find/artist/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ArtistDetailsPage,
});
