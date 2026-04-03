import { createFileRoute } from "@tanstack/react-router";
import ArtistDetailsPage from "@/pages/customer/find/ArtistDetailsPage";

export const Route = createFileRoute("/_customer/find/artist/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ArtistDetailsPage,
});
