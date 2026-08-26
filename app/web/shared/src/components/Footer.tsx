import { Link } from "@tanstack/react-router";
import { ManageCookiesButton } from "@/components/ManageCookiesButton";

export function Footer() {
  return (
    <footer className="border-primary bg-primary border-t">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <Link to="/">
          <img
            src="/logo-long.png"
            alt="Concertable"
            className="h-8 invert-0"
          />
        </Link>
        <Link
          to="/find"
          className="text-primary-foreground/70 hover:text-primary-foreground text-sm font-semibold transition-colors"
        >
          Find Events/Artists/Venues
        </Link>
      </div>
      <div className="border-primary-foreground/20 text-primary-foreground/70 flex flex-col items-center gap-1 border-t py-2 text-center text-xs">
        <nav className="flex flex-wrap items-center justify-center gap-x-4 gap-y-1">
          <a href="/cookies" className="hover:text-foreground transition-colors">
            Cookie policy
          </a>
          <a href="/privacy" className="hover:text-foreground transition-colors">
            Privacy
          </a>
          <ManageCookiesButton className="text-xs" />
        </nav>
        <div>
          Contact:{" "}
          <a
            href="mailto:T.J.Seery-21@student.lboro.ac.uk"
            className="hover:text-foreground transition-colors"
          >
            T.J.Seery-21@student.lboro.ac.uk
          </a>
        </div>
      </div>
    </footer>
  );
}
