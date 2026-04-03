import { createFileRoute, Outlet } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";
import { useVenueNotifications } from "@/hooks/useNotifications";

function VenueLayout() {
  useVenueNotifications();
  return <Outlet />;
}

export const Route = createFileRoute("/venue")({
  beforeLoad: () => requireRole("VenueManager"),
  component: VenueLayout,
});
