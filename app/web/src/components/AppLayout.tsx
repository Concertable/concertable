import { useState, useCallback } from "react";
import { Outlet } from "@tanstack/react-router";
import { Navbar, type NavLink } from "@/components/Navbar";
import { Breadcrumbs } from "@/components/Breadcrumbs";
import { Footer } from "@/components/Footer";
import { NavbarHeightContext } from "@/context/NavbarHeightContext";

interface Props {
  links: NavLink[];
}

export function AppLayout({ links }: Readonly<Props>) {
  const [navbarHeight, setNavbarHeight] = useState(0);
  const [configHeight, setConfigHeight] = useState(0);

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
        <Navbar links={links} onHeightChange={setNavbarHeight} />
        <Breadcrumbs />
        <main className="flex flex-1 flex-col">
          <Outlet />
        </main>
        <Footer />
      </div>
    </NavbarHeightContext.Provider>
  );
}
