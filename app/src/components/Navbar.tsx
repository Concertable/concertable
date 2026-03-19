import { Link } from "@tanstack/react-router";
import { ThemeToggle } from "@/components/ThemeToggle";

export interface NavLink {
  label: string;
  to: string;
}

interface NavbarProps {
  links: NavLink[];
}

export function Navbar({ links }: NavbarProps) {
  return (
    <nav className="flex items-center justify-between border-b border-border px-6 py-3">
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
              className="text-sm text-muted-foreground transition-colors hover:text-foreground [&.active]:text-foreground [&.active]:font-medium"
            >
              {link.label}
            </Link>
          ))}
        </div>
      </div>

      <ThemeToggle />
    </nav>
  );
}
