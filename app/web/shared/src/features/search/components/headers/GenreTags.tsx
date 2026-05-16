import { Music } from "lucide-react";
import type { Genre } from "@/types/common";

interface Props {
  genres: Genre[];
}

export function GenreTags({ genres }: Readonly<Props>) {
  if (genres.length === 0) return null;

  const display = genres
    .slice(0, 3)
    .map((g) => g.name)
    .join(", ");

  return (
    <span className="text-muted-foreground flex items-center gap-1 text-xs">
      <Music className="size-3 shrink-0" />
      {display}
    </span>
  );
}
