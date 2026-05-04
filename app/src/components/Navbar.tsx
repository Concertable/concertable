import { useRef } from "react";

import { Link } from "@tanstack/react-router";
import { ThemeToggle } from "@/components/ThemeToggle";
import { ProfileMenu } from "@/components/ProfileMenu";
import { Mailbox } from "@/features/messaging";
import { NavbarSearch } from "@/features/search";
import { useRole, useRouteRole, useAuthStore } from "@/features/auth";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";

export interface NavLink {
  label: string;
  to: string;
}

interface NavbarProps {
  links: NavLink[];
  onHeightChange: (height: number) => void;
}

export function Navbar({ links, onHeightChange }: Readonly<NavbarProps>) {
  const role = useRole();
  const baseUrl = useAuthStore((s) => s.user?.baseUrl ?? "/");
  const routeRole = useRouteRole();
  const ref = useRef<HTMLElement>(null);

  useMountLayoutEffect(() => {
    if (ref.current) onHeightChange(ref.current.offsetHeight);
  });

  return (
    <nav
      ref={ref}
      className="bg-primary border-primary sticky top-0 z-20 flex items-center justify-between border-b px-6 py-3"
    >
      <div className="flex items-center gap-8">
        <Link to={baseUrl}>
          <img
            src="/logo-long.png"
            alt="Concertable"
            className="hidden h-8 invert-0 sm:block"
          />
          <img
            src="/logo.png"
            alt="Concertable"
            className="block h-8 invert-0 sm:hidden"
          />
        </Link>

        <div className="flex items-center gap-6">
          {links.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              activeOptions={{ exact: true }}
              className="text-primary-foreground/70 hover:text-primary-foreground [&.active]:text-primary-foreground text-sm transition-colors [&.active]:font-medium"
              data-testid={link.label.toLowerCase().replace(/\s+/g, "-")}
            >
              {link.label}
            </Link>
          ))}
        </div>
      </div>

      <div className="text-primary-foreground flex items-center gap-2 [&_button]:hover:bg-white/10">
        {routeRole === "customer" && <NavbarSearch />}
        {role && <Mailbox />}
        <ThemeToggle />
        <ProfileMenu />
      </div>
    </nav>
  );
}
