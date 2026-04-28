import type { ReactNode } from "react";

interface Props {
  title?: string;
  banner?: ReactNode;
  children: ReactNode;
  summary: ReactNode;
}

export function CheckoutLayout({
  title = "Checkout",
  banner,
  children,
  summary,
}: Props) {
  return (
    <div className="mx-auto max-w-5xl px-6 py-8 lg:px-8">
      <div className="grid gap-10 lg:grid-cols-[minmax(0,1fr)_360px] lg:gap-12">
        <div>
          <h1 className="mb-2 text-2xl font-bold tracking-tight">{title}</h1>
          {banner && <div className="mb-6">{banner}</div>}
          {children}
        </div>
        <aside className="lg:sticky lg:top-8 lg:h-fit">{summary}</aside>
      </div>
    </div>
  );
}
