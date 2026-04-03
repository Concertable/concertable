import { createFileRoute, Outlet } from "@tanstack/react-router";
import { requireRole } from "@/lib/guards";
import { useArtistNotifications } from "@/hooks/useNotifications";

function ArtistLayout() {
  useArtistNotifications();
  return <Outlet />;
}

export const Route = createFileRoute("/artist")({
  beforeLoad: () => requireRole("ArtistManager"),
  component: ArtistLayout,
});
