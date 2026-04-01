import { createRootRoute, Outlet, useRouterState } from "@tanstack/react-router";

import { TanStackRouterDevtools } from "@tanstack/router-devtools";
import { Toaster } from "@/components/ui/sonner";
import { Navbar } from "@/components/Navbar";
import { Breadcrumbs } from "@/components/Breadcrumbs";
import type { NavLink } from "@/components/Navbar";
import type { UserRole } from "@/types/auth";
import { useNavSection } from "@/hooks/useNavSection";
import { useSignalR } from "@/hooks/useSignalR";

const navLinks: Record<UserRole, NavLink[]> = {
  Customer: [
    { label: "Home", to: "/" },
    { label: "Find", to: "/find" },
    { label: "Profile", to: "/profile" },
  ],
  ArtistManager: [
    { label: "Dashboard", to: "/artist" },
    { label: "My Concerts", to: "/artist/my" },
    { label: "Find Venues", to: "/artist/find" },
  ],
  VenueManager: [
    { label: "Dashboard", to: "/venue" },
    { label: "My Venue", to: "/venue/my" },
    { label: "My Concerts", to: "/venue/my/concerts" },
    { label: "Find Artists", to: "/venue/find" },
  ],
};

const authRoutes = ["/login", "/register"];

function RootLayout() {
  useSignalR();
  const section = useNavSection();
  const links = navLinks[section];
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const showNav = !authRoutes.includes(pathname);

  return (
    <>
      {showNav && <Navbar links={links} />}
      {showNav && <Breadcrumbs />}
      <Outlet />
      <Toaster richColors />
      {import.meta.env.DEV && <TanStackRouterDevtools />}
    </>
  );
}

export const Route = createRootRoute({
  component: RootLayout,
});
