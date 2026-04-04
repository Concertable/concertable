import type { Header } from "@/types/header";
import type { ReactNode } from "react";
import { Link } from "@tanstack/react-router";
import { MapPin, Star } from "lucide-react";

interface Props<T extends Header> {
  data: T;
  to?: string;
  children?: ReactNode;
}

export function HeaderCard<T extends Header>({ data, to, children }: Readonly<Props<T>>) {
  const content = (
    <div className="flex flex-col items-center text-center rounded-xl border border-border bg-card p-4 gap-2 w-full cursor-pointer transition-shadow hover:shadow-lg">
      <div className="flex items-center gap-1 text-sm font-semibold">
        <span className="line-clamp-1">{data.name}</span>
        {data.rating && (
          <span className="flex items-center gap-0.5 text-yellow-400 shrink-0">
            <Star className="size-3 fill-yellow-400" />
            {data.rating}
          </span>
        )}
      </div>
      <img
        src={data.imageUrl}
        alt={data.name}
        className="w-[180px] h-[180px] object-cover rounded-lg"
      />
      <span className="flex items-center gap-1 text-xs text-muted-foreground">
        <MapPin className="size-3 shrink-0" />
        {data.county}, {data.town}
      </span>
      {children}
    </div>
  );

  if (to) return <Link to={to}>{content}</Link>;
  return content;
}
