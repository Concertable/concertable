import { Link, useMatches } from "@tanstack/react-router";

function formatSegment(segment: string) {
  return segment
    .replace(/^\$/, "")
    .replace(/-/g, " ")
    .replace(/\b\w/g, (c) => c.toUpperCase());
}

export function Breadcrumbs() {
  const matches = useMatches();

  const crumbs = matches
    .filter((m) => m.pathname !== "/")
    .map((m) => {
      const segment = m.pathname.split("/").filter(Boolean).at(-1) ?? "";
      return { label: formatSegment(segment), to: m.pathname };
    });

  if (crumbs.length === 0) return null;

  return (
    <div className="text-muted-foreground flex items-center gap-1 px-6 py-2 text-sm">
      <Link to="/" className="hover:text-secondary transition-colors">
        Home
      </Link>
      {crumbs.map((crumb) => (
        <span key={crumb.to} className="flex items-center gap-1">
          <span>/</span>
          <Link
            to={crumb.to}
            className="hover:text-secondary [&.active]:text-secondary transition-colors [&.active]:font-medium"
          >
            {crumb.label}
          </Link>
        </span>
      ))}
    </div>
  );
}
