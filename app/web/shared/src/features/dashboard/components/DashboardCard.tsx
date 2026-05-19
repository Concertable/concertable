import { Link } from "@tanstack/react-router";
import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface DashboardCardProps {
  title: string;
  icon?: LucideIcon;
  actionLabel?: string;
  actionHref?: string;
  footerLabel?: string;
  footerHref?: string;
  className?: string;
  bodyClassName?: string;
  children: React.ReactNode;
}

export function DashboardCard({
  title,
  icon: Icon,
  actionLabel,
  actionHref,
  footerLabel,
  footerHref,
  className,
  bodyClassName,
  children,
}: DashboardCardProps) {
  return (
    <section
      className={cn(
        "bg-card flex h-full flex-col rounded-lg border p-4 shadow-sm",
        className,
      )}
    >
      <header className="mb-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          {Icon && <Icon className="text-muted-foreground size-4" />}
          <h2 className="text-sm font-semibold">{title}</h2>
        </div>
        {actionLabel && actionHref && (
          <Link
            to={actionHref}
            className="text-primary text-xs font-medium hover:underline"
          >
            {actionLabel} →
          </Link>
        )}
      </header>

      <div className={cn("flex-1", bodyClassName)}>{children}</div>

      {footerLabel && footerHref && (
        <footer className="border-border mt-3 border-t pt-2">
          <Link
            to={footerHref}
            className="text-primary text-xs font-medium hover:underline"
          >
            {footerLabel} →
          </Link>
        </footer>
      )}
    </section>
  );
}
