import { useRef, type ReactNode } from "react";

import { Link } from "@tanstack/react-router";
import { Menu, Search } from "lucide-react";
import { ThemeToggle } from "@/components/ThemeToggle";
import { ProfileMenu, type ProfileMenuItem } from "@/components/ProfileMenu";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Mailbox } from "@/features/messaging";
import { NavbarSearch } from "@/features/search";
import { useMeQuery } from "@/features/user";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";

export interface NavLink {
  label: string;
  to?: string;
  href?: string;
}

function NavLinkAnchor({ link, className }: Readonly<{ link: NavLink; className?: string }>) {
  return link.href ? (
    <a
      href={link.href}
      target="_blank"
      rel="noopener noreferrer"
      className={className}
      data-testid={link.label.toLowerCase().replace(/\s+/g, "-")}
    >
      {link.label}
    </a>
  ) : (
    <Link
      to={link.to!}
      activeOptions={{ exact: true }}
      className={className}
      data-testid={link.label.toLowerCase().replace(/\s+/g, "-")}
    >
      {link.label}
    </Link>
  );
}

interface Props {
  links: NavLink[];
  profileItems?: ProfileMenuItem[];
  headerSlot?: ReactNode;
  /** Replaces the default `ProfileMenu` — for a surface where its hardcoded
   * `/settings`/`/settings/payment` links don't apply. */
  profileSlot?: ReactNode;
  /** App-injected content between `headerSlot` and the theme toggle — e.g. messaging. Additive
   * seam for a future minimal-shell `Navbar`; see `app/web/shared/TECH_DEBT.md`. */
  endSlot?: ReactNode;
  showSearch?: boolean;
  showMailbox?: boolean;
  onHeightChange?: (height: number) => void;
}

export function Navbar({
  links,
  profileItems,
  headerSlot,
  profileSlot,
  endSlot,
  showSearch = true,
  showMailbox = true,
  onHeightChange,
}: Readonly<Props>) {
  const { data: user } = useMeQuery();
  const ref = useRef<HTMLElement>(null);

  useMountLayoutEffect(() => {
    if (ref.current) onHeightChange?.(ref.current.offsetHeight);
  });

  return (
    <nav
      ref={ref}
      className="bg-primary border-primary sticky top-0 z-20 flex items-center justify-between gap-3 border-b px-4 py-3 sm:px-6"
    >
      <div className="flex min-w-0 items-center gap-3 sm:gap-8">
        <Link to="/">
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

        <div className="hidden items-center gap-6 md:flex">
          {links.map((link) => (
            <NavLinkAnchor
              key={link.href ?? link.to}
              link={link}
              className="text-primary-foreground/70 hover:text-primary-foreground [&.active]:text-primary-foreground text-sm transition-colors [&.active]:font-medium"
            />
          ))}
        </div>

        <DropdownMenu>
          <DropdownMenuTrigger
            aria-label="Open navigation"
            className="text-primary-foreground hover:bg-white/10 rounded-md p-2 md:hidden"
          >
            <Menu className="size-5" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="start">
            {links.map((link) => (
              <DropdownMenuItem key={link.href ?? link.to} asChild>
                <NavLinkAnchor link={link} />
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="text-primary-foreground flex min-w-0 items-center gap-1 sm:gap-2 [&_button]:hover:bg-white/10">
        {headerSlot}
        {showSearch && (
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
          </>
        )}
        {showMailbox && user && <Mailbox />}
        {endSlot}
        <ThemeToggle />
        {profileSlot ?? <ProfileMenu items={profileItems ?? []} />}
      </div>
    </nav>
  );
}
