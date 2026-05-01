import { createFileRoute } from "@tanstack/react-router";
import { requireRole } from "@/features/auth";
import { useArtistNotifications } from "@/features/notifications";
import { AppLayout } from "@/components/AppLayout";

const links = [
  { label: "Dashboard", to: "/artist" },
  { label: "My Concerts", to: "/artist/my" },
  { label: "Find Venues", to: "/artist/find" },
];

function ArtistLayout() {
  useArtistNotifications();
  return <AppLayout links={links} />;
}

export const Route = createFileRoute("/artist")({
  beforeLoad: () => requireRole("ArtistManager"),
  component: ArtistLayout,
});
