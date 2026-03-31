import type { Artist } from "@/types/artist";
import { MapPin } from "lucide-react";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";

interface Props {
  artist: Artist;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function ArtistDetails({ artist, onNameChange, onAboutChange }: Readonly<Props>) {
  return (
    <div>
      <Hero
        imageUrl={artist.imageUrl}
        name={artist.name}
        town={artist.town}
        county={artist.county}
        namePlaceholder="Artist name"
        onNameChange={onNameChange}
      />

      {/* Sections */}
      <div className="max-w-4xl mx-auto px-6 py-10 space-y-10">
        <section className="space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell venues about yourself..."
          >
            {artist.about}
          </EditableTextarea>
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