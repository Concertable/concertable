import { createFileRoute } from "@tanstack/react-router";
import ArtistDetailsPage from "@/pages/customer/find/ArtistDetailsPage";

export const Route = createFileRoute("/_customer/find/artist/$id")({
  component: ArtistDetailsPage,
});
