import { createFileRoute } from "@tanstack/react-router";
import { requireRole } from "@/features/auth";
import { useVenueNotifications } from "@/features/notifications";
import { AppLayout } from "@/components/AppLayout";

const links = [
  { label: "Dashboard", to: "/venue" },
  { label: "My Venue", to: "/venue/my" },
  { label: "My Concerts", to: "/venue/my/concerts" },
  { label: "Find Artists", to: "/venue/find" },
];

function VenueLayout() {
  useVenueNotifications();
  return <AppLayout links={links} />;
}

export const Route = createFileRoute("/venue")({
  beforeLoad: () => requireRole("VenueManager"),
  component: VenueLayout,
});
