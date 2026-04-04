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
    <nav ref={ref} className="sticky top-0 z-20 bg-background flex items-center justify-between border-b border-border px-6 py-3">
      <div className="flex items-center gap-8">
        <Link to={baseUrl}>
          <img src="/logo-long.png" alt="Concertable" className="h-8 dark:invert-0 invert hidden sm:block" />
          <img src="/logo.png" alt="Concertable" className="h-8 dark:invert-0 invert block sm:hidden" />
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
