import { createFileRoute } from "@tanstack/react-router";
import LocationPage from "@/pages/customer/profile/LocationPage";

export const Route = createFileRoute("/_customer/profile/location")({
  component: LocationPage,
});
