import { useLayoutEffect, useRef } from "react";
import { Link } from "@tanstack/react-router";
import { ThemeToggle } from "@/components/ThemeToggle";
import { UserMenu } from "@/components/UserMenu";
import { Mailbox } from "@/components/Mailbox";
import { useRole } from "@/hooks/useRole";

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
  const ref = useRef<HTMLElement>(null);

  useLayoutEffect(() => {
    if (ref.current) onHeightChange(ref.current.offsetHeight);
  }, []);

  return (
    <nav ref={ref} className="sticky top-0 z-20 bg-background flex items-center justify-between border-b border-border px-6 py-3">
      <div className="flex items-center gap-8">
        <Link to="/">
          <img
            src="/logo-long.png"
            alt="Concertable"
            className="h-8 dark:invert-0 invert"
          />
        </Link>

        <div className="flex items-center gap-6">
          {links.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              activeOptions={{ exact: true }}
              className="text-sm text-muted-foreground transition-colors hover:text-foreground [&.active]:text-foreground [&.active]:font-medium"
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
