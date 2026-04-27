import type { ReactNode } from "react";

interface Props {
  title?: string;
  children: ReactNode;
  summary: ReactNode;
}

export function CheckoutLayout({
  title = "Checkout",
  children,
  summary,
}: Props) {
  return (
    <div className="mx-auto max-w-7xl px-6 py-10 lg:px-12 lg:py-16">
      <h1 className="mb-10 text-3xl font-bold tracking-tight">{title}</h1>
      <div className="grid gap-12 lg:grid-cols-[minmax(0,1fr)_420px] lg:gap-20">
        <div>{children}</div>
        <aside className="lg:sticky lg:top-12 lg:h-fit">{summary}</aside>
      </div>
    </div>
  );
}
