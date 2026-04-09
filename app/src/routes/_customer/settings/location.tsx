import { createFileRoute } from "@tanstack/react-router";
import LocationPage from "@/pages/customer/profile/LocationPage";
import { requireRole } from "@/lib/guards";

export const Route = createFileRoute("/_customer/settings/location")({
  beforeLoad: () => requireRole("Customer"),
  component: LocationPage,
});
