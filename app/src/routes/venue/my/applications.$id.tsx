import { createFileRoute } from "@tanstack/react-router";
import ListingApplicationsPage from "@/pages/venue/my/ListingApplicationsPage";

export const Route = createFileRoute("/venue/my/applications/$id")({
  component: ListingApplicationsPage,
});
