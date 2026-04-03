import { createFileRoute } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";
import { useArtistNotifications } from "@/hooks/useNotifications";
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
