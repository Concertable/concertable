import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  XAxis,
  YAxis,
} from "recharts";
import dayjs from "dayjs";
import type { MonthlyRevenuePoint } from "@concertable/shared/features/dashboard";
import { formatCurrency } from "@concertable/shared/lib";
import { ChartTooltip } from "./ChartTooltip";

interface MonthlyRevenueChartProps {
  data: MonthlyRevenuePoint[];
  accent?: "emerald" | "sky";
  currency?: string;
}

const accentColors = {
  emerald: { gross: "#10b981", net: "#059669" },
  sky: { gross: "#0ea5e9", net: "#0284c7" },
};

export function MonthlyRevenueChart({
  data,
  accent = "emerald",
  currency = "GBP",
}: MonthlyRevenueChartProps) {
  const colors = accentColors[accent];
  const chartData = data.map((p) => ({
    month: dayjs(p.month).format("MMM"),
    gross: p.grossCents / 100,
    net: p.netCents / 100,
    count: p.count,
  }));

  return (
    <ResponsiveContainer width="100%" height={180}>
      <AreaChart data={chartData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
        <defs>
          <linearGradient id={`fill-${accent}-gross`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={colors.gross} stopOpacity={0.4} />
            <stop offset="100%" stopColor={colors.gross} stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />
        <XAxis
          dataKey="month"
          tickLine={false}
          axisLine={false}
          fontSize={11}
          stroke="hsl(var(--muted-foreground))"
        />
        <YAxis
          tickFormatter={(v: number) => formatCurrency(v * 100, { currency, compact: true })}
          tickLine={false}
          axisLine={false}
          fontSize={11}
          width={50}
          tickCount={4}
          stroke="hsl(var(--muted-foreground))"
        />
        <ChartTooltip currency={currency} />
        <Area
          type="monotone"
          dataKey="gross"
          stroke={colors.gross}
          fill={`url(#fill-${accent}-gross)`}
          strokeWidth={2}
        />
        <Area
          type="monotone"
          dataKey="net"
          stroke={colors.net}
          fill="transparent"
          strokeWidth={2}
          strokeDasharray="4 4"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}
