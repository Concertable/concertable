import type { ReactNode } from "react";
import { Separator } from "@/components/ui/separator";

export interface SummaryLine {
  label: string;
  value: ReactNode;
}

interface Props {
  title?: string;
  lines: SummaryLine[];
  total: SummaryLine;
  action: ReactNode;
  footer?: ReactNode;
}

export function OrderSummaryCard({
  title = "Summary",
  lines,
  total,
  action,
  footer,
}: Props) {
  return (
    <div className="bg-muted/30 space-y-4 rounded-lg border p-5">
      <h2 className="text-base font-semibold tracking-tight">{title}</h2>
      <div className="space-y-2">
        {lines.map((l) => (
          <div key={l.label} className="flex justify-between text-sm">
            <span className="text-muted-foreground">{l.label}</span>
            <span>{l.value}</span>
          </div>
        ))}
      </div>
      <Separator />
      <div className="flex flex-wrap items-baseline justify-between gap-x-4 gap-y-1">
        <span className="text-sm font-medium">{total.label}</span>
        <span className="ml-auto text-2xl font-semibold tracking-tight">
          {total.value}
        </span>
      </div>
      {action}
      {footer}
    </div>
  );
}
