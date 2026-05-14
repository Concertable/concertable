import type { ReactNode } from "react";

interface Props {
  title: string;
  subtitle?: string;
  meta?: string;
  trailing?: ReactNode;
}

export function CheckoutEventBanner({
  title,
  subtitle,
  meta,
  trailing,
}: Props) {
  return (
    <div className="flex items-center justify-between gap-4 border-b pb-4">
      <div className="min-w-0">
        <p className="font-semibold tracking-tight">{title}</p>
        {subtitle && (
          <p className="text-muted-foreground truncate text-sm">{subtitle}</p>
        )}
        {meta && <p className="text-muted-foreground text-xs">{meta}</p>}
      </div>
      {trailing && <div className="shrink-0">{trailing}</div>}
    </div>
  );
}
