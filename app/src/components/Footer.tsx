import { Link } from "@tanstack/react-router";

export function Footer() {
  return (
    <footer className="border-t border-border bg-background">
      <div className="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between">
        <Link to="/">
          <img src="/logo-long.png" alt="Concertable" className="h-8 dark:invert-0 invert" />
        </Link>
        <Link to="/find" className="text-sm font-semibold text-muted-foreground hover:text-foreground transition-colors">
          Find Events/Artists/Venues
        </Link>
      </div>
      <div className="border-t border-border py-2 text-center text-xs text-muted-foreground">
        Contact:{" "}
        <a href="mailto:T.J.Seery-21@student.lboro.ac.uk" className="hover:text-foreground transition-colors">
          T.J.Seery-21@student.lboro.ac.uk
        </a>
      </div>
    </footer>
  );
}
