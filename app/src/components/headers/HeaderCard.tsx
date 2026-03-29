import type { Header } from "@/types/header";
import type { ReactNode } from "react";
import { Link } from "@tanstack/react-router";

interface Props<T extends Header> {
  data: T;
  to?: string;
  children?: ReactNode;
}

export function HeaderCard<T extends Header>({ data, to, children }: Readonly<Props<T>>) {
  const content = (
    <div className="rounded-lg border border-border overflow-hidden bg-background">
      <img src={data.imageUrl} alt={data.name} className="w-full h-40 object-cover" />
      <div className="p-3">
        <div className="flex items-center justify-between">
          <span className="font-medium text-sm">{data.name}</span>
          {data.rating && <span className="text-sm text-muted-foreground">⭐ {data.rating}</span>}
        </div>
        <p className="text-xs text-muted-foreground mt-1">{data.town}, {data.county}</p>
        {children}
      </div>
    </div>
  );

  if (to) return <Link to={to}>{content}</Link>;
  return content;
}
