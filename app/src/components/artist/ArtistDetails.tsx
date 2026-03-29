import type { Artist } from "@/types/artist";
import { MapPin, Star } from "lucide-react";
import { EditableText } from "@/components/editable/EditableText";
import { EditableTextarea } from "@/components/editable/EditableTextarea";

interface Props {
  artist: Artist;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function ArtistDetails({ artist, onNameChange, onAboutChange }: Readonly<Props>) {
  return (
    <div>
      {/* Hero */}
      <div className="relative bg-muted h-72 flex items-end">
        {artist.imageUrl && (
          <img
            src={artist.imageUrl}
            alt={artist.name}
            className="absolute inset-0 w-full h-full object-cover opacity-60"
          />
        )}
        <div className="relative z-10 flex items-end justify-between w-full px-8 pb-6">
          <div className="space-y-1">
            <EditableText
              value={artist.name}
              onChange={onNameChange}
              element="h1"
              placeholder="Artist name"
              className="text-3xl font-bold text-white drop-shadow"
            />
            {(artist.town || artist.county) && (
              <p className="flex items-center gap-1 text-white/80 text-sm drop-shadow">
                <MapPin className="size-4" />
                {[artist.town, artist.county].filter(Boolean).join(", ")}
              </p>
            )}
          </div>
          <div className="flex items-center gap-1 text-white/80 text-sm drop-shadow">
            <Star className="size-4 fill-yellow-400 text-yellow-400" />
            <span>No reviews yet</span>
          </div>
        </div>
      </div>

      {/* Sections */}
      <div className="max-w-4xl mx-auto px-6 py-10 space-y-10">
        <section className="space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            value={artist.about}
            onChange={onAboutChange}
            placeholder="Tell venues about yourself..."
          />
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Location</h2>
          <p className="flex items-center gap-2 text-muted-foreground">
            <MapPin className="size-4" />
            {[artist.town, artist.county].filter(Boolean).join(", ") || "No location set."}
          </p>
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Concerts</h2>
          <p className="text-muted-foreground">No upcoming concerts.</p>
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Opportunities</h2>
          <p className="text-muted-foreground">No opportunities yet.</p>
        </section>
      </div>
    </div>
  );
}