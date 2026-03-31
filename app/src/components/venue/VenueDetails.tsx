import type { Venue } from "@/types/venue";
import { MapPin } from "lucide-react";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ReviewSection } from "@/components/reviews/ReviewSection";

interface Props {
  venue: Venue;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function VenueDetails({ venue, onNameChange, onAboutChange }: Readonly<Props>) {
  return (
    <div>
      <Hero
        imageUrl={venue.imageUrl}
        name={venue.name}
        town={venue.town}
        county={venue.county}
        namePlaceholder="Venue name"
        onNameChange={onNameChange}
      />

      {/* Sections */}
      <div className="max-w-4xl mx-auto px-6 py-10 space-y-10">
        <section className="space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell artists about your venue..."
          >
            {venue.about}
          </EditableTextarea>
        </section>

        <div className="border-t border-border" />

        <section className="space-y-2">
          <h2 className="text-xl font-semibold">Location</h2>
          <p className="flex items-center gap-2 text-muted-foreground">
            <MapPin className="size-4" />
            {[venue.town, venue.county].filter(Boolean).join(", ") || "No location set."}
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

        <div className="border-t border-border" />

        <ReviewSection type="venue" id={venue.id} />
      </div>
    </div>
  );
}