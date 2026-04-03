import { createFileRoute } from "@tanstack/react-router";
import VenueDetailsPage from "@/pages/customer/find/VenueDetailsPage";

export const Route = createFileRoute("/_customer/find/venue/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: VenueDetailsPage,
});
