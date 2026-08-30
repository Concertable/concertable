import { useState, useCallback, type ReactNode } from "react";
import { Outlet } from "@tanstack/react-router";
import { Navbar, type NavLink } from "@/components/Navbar";
import type { ProfileMenuItem } from "@/components/ProfileMenu";
import { Breadcrumbs } from "@/components/Breadcrumbs";
import { Footer } from "@/components/Footer";
import { NavbarHeightContext } from "@/context/NavbarHeightContext";
import { useMeQuery } from "@/features/user";

interface Props {
  links: NavLink[];
  profileItems: ProfileMenuItem[];
  headerSlot?: ReactNode;
  /** Additive seam for a future migration onto `Navbar`'s `endSlot` — not yet wired to any
   * caller. See `app/web/TECH_DEBT.md`'s Mailbox entry. */
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
          endSlot={user && messagingSlot}
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
