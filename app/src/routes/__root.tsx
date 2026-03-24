import { createRootRoute, Outlet, useRouterState } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/router-devtools";
import { Toaster } from "@/components/ui/sonner";
import { Navbar } from "@/components/Navbar";
import { Breadcrumbs } from "@/components/Breadcrumbs";
import { useAuthStore } from "@/store/useAuthStore";
import type { NavLink } from "@/components/Navbar";

const navLinks: Record<string, NavLink[]> = {
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
    { label: "My Concerts", to: "/venue/my" },
    { label: "Find Artists", to: "/venue/find" },
  ],
};

const authRoutes = ["/login", "/register"];

function RootLayout() {
  const user = useAuthStore((s) => s.user);
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const links = (user?.role ? navLinks[user.role] : null) ?? navLinks.Customer;
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
