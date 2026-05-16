import type { ReactNode } from "react";

interface Props {
  title: string;
  description?: string;
  children: ReactNode;
}

export function CheckoutSection({ title, description, children }: Props) {
  return (
    <section className="space-y-3 py-4 first:pt-0 [&:not(:first-child)]:border-t">
      <header className="space-y-0.5">
        <h2 className="text-base font-semibold tracking-tight">{title}</h2>
        {description && (
          <p className="text-muted-foreground text-sm">{description}</p>
        )}
      </header>
      {children}
    </section>
  );
}
