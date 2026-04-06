import type { Venue } from "@/types/venue";
import { MapPin } from "lucide-react";
import { GoogleMap } from "@/components/GoogleMap";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ReviewSection } from "@/components/reviews/ReviewSection";
import { OpportunitySection } from "@/components/opportunities/OpportunitySection";
import { ScrollspyNav } from "@/components/ScrollspyNav";

const SECTIONS = [
  { id: "about", label: "About" },
  { id: "location", label: "Location" },
  { id: "concerts", label: "Concerts" },
  { id: "opportunities", label: "Opportunities" },
  { id: "reviews", label: "Reviews" },
];

interface Props {
  venue: Venue;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function VenueDetails({
  venue,
  onNameChange,
  onAboutChange,
}: Readonly<Props>) {
  return (
    <div>
      <Hero
        imageUrl={venue.bannerUrl}
        name={venue.name}
        town={venue.town}
        county={venue.county}
        namePlaceholder="Venue name"
        onNameChange={onNameChange}
      />

      <ScrollspyNav sections={SECTIONS} />

      <div className="mx-auto max-w-4xl space-y-10 px-6 py-10">
        <section id="about" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell artists about your venue..."
          >
            {venue.about}
          </EditableTextarea>
        </section>

        <div className="border-border border-t" />

        <section id="location" className="scroll-mt-24 space-y-3">
          <h2 className="text-xl font-semibold">Location</h2>
          <p className="text-muted-foreground flex items-center gap-2">
            <MapPin className="size-4" />
            {[venue.town, venue.county].filter(Boolean).join(", ") ||
              "No location set."}
          </p>
          <GoogleMap
            className="mt-3"
            lat={venue.latitude}
            lng={venue.longitude}
          />
        </section>

        <div className="border-border border-t" />

        <section id="concerts" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">Concerts</h2>
          <p className="text-muted-foreground">No upcoming concerts.</p>
        </section>

        <div className="border-border border-t" />

        <section id="opportunities" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">Opportunities</h2>
          <OpportunitySection venueId={venue.id} />
        </section>

        <div className="border-border border-t" />

        <section id="reviews" className="scroll-mt-24">
          <ReviewSection type="venue" id={venue.id} />
        </section>
      </div>
    </div>
  );
}
