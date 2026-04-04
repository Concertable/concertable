import { useRef } from "react";
import { Link } from "@tanstack/react-router";
import { ThemeToggle } from "@/components/ThemeToggle";
import { UserMenu } from "@/components/UserMenu";
import { Mailbox } from "@/components/Mailbox";
import { useRole } from "@/hooks/useRole";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";
import { useAuthStore } from "@/store/useAuthStore";

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
  const ref = useRef<HTMLElement>(null);

  useMountLayoutEffect(() => {
    if (ref.current) onHeightChange(ref.current.offsetHeight);
  });

  return (
    <nav
      ref={ref}
      className="bg-background border-border sticky top-0 z-20 flex items-center justify-between border-b px-6 py-3"
    >
      <div className="flex items-center gap-8">
        <Link to={baseUrl}>
          <img
            src="/logo-long.png"
            alt="Concertable"
            className="hidden h-8 invert sm:block dark:invert-0"
          />
          <img
            src="/logo.png"
            alt="Concertable"
            className="block h-8 invert sm:hidden dark:invert-0"
          />
        </Link>

        <div className="flex items-center gap-6">
          {links.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              activeOptions={{ exact: true }}
              className="text-muted-foreground hover:text-foreground [&.active]:text-foreground text-sm transition-colors [&.active]:font-medium"
            >
              {link.label}
            </Link>
          ))}
        </div>
      </div>

      <div className="flex items-center gap-2">
        {role && <Mailbox />}
        <ThemeToggle />
        <UserMenu />
      </div>
    </nav>
  );
}
