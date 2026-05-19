import { Link } from "@tanstack/react-router";
import { Check, Circle, ShieldCheck } from "lucide-react";
import type { ProfileHealth } from "@concertable/shared/features/dashboard";
import { cn } from "@/lib/utils";

export function ProfileHealthCard({ health }: { health: ProfileHealth }) {
  return (
    <section className="bg-card flex flex-col gap-3 rounded-lg border p-4 shadow-sm">
      <header className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <ShieldCheck className="text-muted-foreground size-4" />
          <h2 className="text-sm font-semibold">Profile health</h2>
        </div>
        <span className="text-muted-foreground text-xs tabular-nums">
          {health.completeness}% complete
        </span>
      </header>

      <div className="bg-muted relative h-1.5 w-full overflow-hidden rounded-full">
        <div
          className={cn(
            "absolute inset-y-0 left-0 transition-all",
            health.completeness === 100 ? "bg-emerald-500" : "bg-sky-500",
          )}
          style={{ width: `${health.completeness}%` }}
        />
      </div>

      <ul className="flex flex-col gap-1.5">
        {health.items.map((item) => (
          <li key={item.id}>
            {item.done ? (
              <div className="text-muted-foreground flex items-center gap-2 text-sm">
                <Check className="size-4 text-emerald-600" />
                <span className="line-through">{item.label}</span>
              </div>
            ) : (
              <Link
                to={item.href}
                className="hover:text-foreground flex items-center gap-2 text-sm"
              >
                <Circle className="text-muted-foreground size-4" />
                <span>{item.label}</span>
              </Link>
            )}
          </li>
        ))}
      </ul>
    </section>
  );
}
