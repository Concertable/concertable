import { createFileRoute } from "@tanstack/react-router";
import ListingApplicationsPage from "@/pages/venue/my/ListingApplicationsPage";

export const Route = createFileRoute("/venue/my/applications/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ListingApplicationsPage,
});
