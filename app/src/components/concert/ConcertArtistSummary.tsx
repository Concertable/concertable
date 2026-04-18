import { MapPin, Star } from "lucide-react";
import type { ConcertArtist } from "@/types/concert";
import { GenreTags } from "@/components/headers/GenreTags";
import { useImageUrl } from "@/hooks/query/useImageUrl";

interface Props {
  artist: ConcertArtist;
}

export function ConcertArtistSummary({ artist }: Readonly<Props>) {
  const { data: src } = useImageUrl(artist.avatar);

  return (
    <div className="flex items-center gap-4">
      {src && (
        <img
          src={src}
          alt={artist.name}
          className="size-14 rounded-full object-cover"
        />
      )}
      <div className="space-y-1">
        <div className="flex items-center gap-1 font-semibold">
          <span>{artist.name}</span>
          {artist.rating > 0 && (
            <span className="text-gold flex items-center gap-0.5 text-sm">
              <Star className="fill-gold size-3" />
              {artist.rating}
            </span>
          )}
        </div>
        <span className="text-muted-foreground flex items-center gap-1 text-xs">
          <MapPin className="size-3 shrink-0" />
          {artist.county}, {artist.town}
        </span>
        <GenreTags genres={artist.genres} />
      </div>
    </div>
  );
}
