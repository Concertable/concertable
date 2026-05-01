import type { ReactNode } from "react";
import { Link } from "@tanstack/react-router";
import { MapPin, Star } from "lucide-react";
import { useImageUrl } from "@/hooks/query/useImageUrl";
import type { Header } from "../../types";

interface Props<T extends Header> {
  data: T;
  to?: string;
  children?: ReactNode;
}

export function HeaderCard<T extends Header>({
  data,
  to,
  children,
}: Readonly<Props<T>>) {
  const { data: src } = useImageUrl(data.imageUrl);

  const content = (
    <div className="border-border bg-card flex w-full cursor-pointer flex-col items-center gap-2 rounded-xl border p-4 text-center transition-shadow hover:shadow-lg">
      <div className="flex items-center gap-1 text-sm font-semibold">
        <span className="line-clamp-1">{data.name}</span>
        {data.rating && (
          <span className="text-gold flex shrink-0 items-center gap-0.5">
            <Star className="fill-gold size-3" />
            {data.rating}
          </span>
        )}
      </div>
      <img
        src={src}
        alt={data.name}
        className="h-[180px] w-[180px] rounded-lg object-cover"
      />
      <span className="text-muted-foreground flex items-center gap-1 text-xs">
        <MapPin className="size-3 shrink-0" />
        {data.county}, {data.town}
      </span>
      {children}
    </div>
  );

  if (to) return <Link to={to}>{content}</Link>;
  return content;
}
