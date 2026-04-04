import { Link } from "@tanstack/react-router";

export function Footer() {
  return (
    <footer className="border-border bg-background border-t">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <Link to="/">
          <img
            src="/logo-long.png"
            alt="Concertable"
            className="h-8 invert dark:invert-0"
          />
        </Link>
        <Link
          to="/find"
          className="text-muted-foreground hover:text-foreground text-sm font-semibold transition-colors"
        >
          Find Events/Artists/Venues
        </Link>
      </div>
      <div className="border-border text-muted-foreground border-t py-2 text-center text-xs">
        Contact:{" "}
        <a
          href="mailto:T.J.Seery-21@student.lboro.ac.uk"
          className="hover:text-foreground transition-colors"
        >
          T.J.Seery-21@student.lboro.ac.uk
        </a>
      </div>
    </footer>
  );
}
