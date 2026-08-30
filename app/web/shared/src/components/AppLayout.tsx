import { useState, useCallback, type ReactNode } from "react";
import { Outlet, Link } from "@tanstack/react-router";
import { Search } from "lucide-react";
import { Navbar, type NavLink } from "@/components/Navbar";
import type { ProfileMenuItem } from "@/components/ProfileMenu";
import { Breadcrumbs } from "@/components/Breadcrumbs";
import { Footer } from "@/components/Footer";
import { NavbarHeightContext } from "@/context/NavbarHeightContext";
import { NavbarSearch } from "@/features/search";
import { useMeQuery } from "@/features/user";

interface Props {
  links: NavLink[];
  profileItems: ProfileMenuItem[];
  headerSlot?: ReactNode;
  /** Injected only by apps whose backend actually serves messaging (venue, artist) — see
   * `app/web/TECH_DEBT.md`'s Mailbox entry for why this can't be a default. */
  messagingSlot?: ReactNode;
}

export function AppLayout({
  links,
  profileItems,
  headerSlot,
  messagingSlot,
}: Readonly<Props>) {
  const [navbarHeight, setNavbarHeight] = useState(0);
  const [configHeight, setConfigHeight] = useState(0);
  const { data: user } = useMeQuery();

  const handleSetConfigHeight = useCallback((height: number) => {
    setConfigHeight(height);
  }, []);

  return (
    <NavbarHeightContext.Provider
      value={{
        navbarHeight,
        totalHeight: navbarHeight + configHeight,
        setConfigHeight: handleSetConfigHeight,
      }}
    >
      <div className="flex min-h-screen flex-col">
        <Navbar
          links={links}
          profileItems={profileItems}
          headerSlot={headerSlot}
          onHeightChange={setNavbarHeight}
          endSlot={
            <>
              <div className="hidden lg:block">
                <NavbarSearch />
              </div>
              <Link
                to="/find"
                aria-label="Search"
                className="hover:bg-white/10 rounded-md p-2 lg:hidden"
              >
                <Search className="size-5" />
              </Link>
              {user && messagingSlot}
            </>
          }
        />
        <Breadcrumbs />
        <main className="flex flex-1 flex-col">
          <Outlet />
        </main>
        <Footer />
      </div>
    </NavbarHeightContext.Provider>
  );
}
