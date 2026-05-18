import { Tooltip } from "recharts";
import { formatCurrency } from "@concertable/shared/lib";

interface ChartTooltipProps {
  currency?: string;
  valueScale?: number;
}

export function ChartTooltip({ currency = "GBP", valueScale = 100 }: ChartTooltipProps) {
  return (
    <Tooltip
      content={({ active, payload, label }) => {
        if (!active || !payload?.length) return null;
        return (
          <div className="bg-popover rounded-md border px-3 py-2 text-xs shadow-md">
            <div className="text-muted-foreground mb-1 font-medium">{label}</div>
            {payload.map((p) => (
              <div key={String(p.dataKey)} className="flex items-center gap-2">
                <span
                  className="size-2 rounded-full"
                  style={{ background: p.color }}
                />
                <span className="text-muted-foreground capitalize">{p.name}</span>
                <span className="ml-auto tabular-nums">
                  {formatCurrency(Number(p.value) * valueScale, {
                    currency,
                    fractionDigits: 2,
                  })}
                </span>
              </div>
            ))}
          </div>
        );
      }}
    />
  );
}
