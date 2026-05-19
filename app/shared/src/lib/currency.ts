interface FormatCurrencyOptions {
  currency?: string;
  compact?: boolean;
  fractionDigits?: number;
}

export function formatCurrency(
  cents: number,
  { currency = "GBP", compact = false, fractionDigits }: FormatCurrencyOptions = {},
): string {
  return new Intl.NumberFormat("en-GB", {
    style: "currency",
    currency,
    notation: compact ? "compact" : "standard",
    maximumFractionDigits: fractionDigits ?? (compact ? 1 : 0),
  }).format(cents / 100);
}
