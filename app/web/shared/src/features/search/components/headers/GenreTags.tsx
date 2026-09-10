import { Music } from "lucide-react";
import { GENRE_LABELS, type Genre } from "@/types/common";

interface Props {
  genres: Genre[];
}

export function GenreTags({ genres }: Readonly<Props>) {
  if (genres.length === 0) return null;

  const display = genres
    .slice(0, 3)
    .map((genre) => GENRE_LABELS[genre])
    .join(", ");

  return (
    <span className="text-muted-foreground flex items-center gap-1 text-xs">
      <Music className="size-3 shrink-0" />
      {display}
    </span>
  );
}
