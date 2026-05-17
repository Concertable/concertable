import { createFileRoute } from "@tanstack/react-router";
import { requireBusinessRole } from "@/features/auth";
import { useArtistNotifications } from "@/features/notifications";
import { AppLayout } from "@/components/AppLayout";

const links = [
  { label: "Dashboard", to: "/" },
  { label: "My Concerts", to: "/my" },
  { label: "Find Venues", to: "/find" },
];

function ArtistLayout() {
  useArtistNotifications();
  return <AppLayout links={links} />;
}

export const Route = createFileRoute("/_artist")({
  beforeLoad: () => requireBusinessRole("ArtistManager"),
  component: ArtistLayout,
});
