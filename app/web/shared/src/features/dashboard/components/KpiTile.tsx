import { Link } from "@tanstack/react-router";
import { ArrowDownRight, ArrowUpRight, type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

export type KpiIntent = "positive" | "urgent" | "pending" | "neutral";

const intentStyles: Record<KpiIntent, string> = {
  positive: "text-emerald-600",
  urgent: "text-rose-600",
  pending: "text-amber-600",
  neutral: "text-sky-600",
};

interface KpiTileProps {
  label: string;
  value: string;
  delta?: { value: string; direction: "up" | "down" };
  intent?: KpiIntent;
  icon?: LucideIcon;
  href: string;
}

export function KpiTile({
  label,
  value,
  delta,
  intent = "neutral",
  icon: Icon,
  href,
}: KpiTileProps) {
  return (
    <Link
      to={href}
      className="bg-card hover:bg-muted/40 group flex flex-col gap-2 rounded-lg border p-4 shadow-sm transition-colors"
    >
      <div className="flex items-center justify-between">
        <span className="text-muted-foreground text-xs font-medium uppercase tracking-wide">
          {label}
        </span>
        {Icon && <Icon className={cn("size-4", intentStyles[intent])} />}
      </div>

      <div className={cn("text-3xl font-semibold tabular-nums", intentStyles[intent])}>
        {value}
      </div>

      {delta && (
        <div
          className={cn(
            "flex items-center gap-1 text-xs",
            delta.direction === "up" ? "text-emerald-600" : "text-rose-600",
          )}
        >
          {delta.direction === "up" ? (
            <ArrowUpRight className="size-3" />
          ) : (
            <ArrowDownRight className="size-3" />
          )}
          <span>{delta.value}</span>
        </div>
      )}
    </Link>
  );
}
