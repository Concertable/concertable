import { ReactNode } from "react";
import { MapPin } from "lucide-react";
import { GoogleMap } from "@/components/GoogleMap";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ScrollspyNav } from "@/components/ScrollspyNav";
import { useVenueStore } from "../store/useVenueStore";
import type { Venue } from "../types";

interface Section {
  id: string;
  label: string;
}

interface Props {
  venue: Venue;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
  sections?: Section[];
  children?: ReactNode;
}

export function VenueDetails({
  venue,
  onNameChange,
  onAboutChange,
  sections,
  children,
}: Readonly<Props>) {
  const setBanner = useVenueStore((s) => s.setBanner);
  const setAvatar = useVenueStore((s) => s.setAvatar);

  return (
    <div>
      <Hero
        bannerUrl={venue.bannerUrl}
        avatar={venue.avatar}
        name={venue.name}
        town={venue.town}
        county={venue.county}
        namePlaceholder="Venue name"
        onNameChange={onNameChange}
        onBannerChange={setBanner}
        onAvatarChange={setAvatar}
      />

      {sections && <ScrollspyNav sections={sections} />}

      <div className="mx-auto max-w-4xl space-y-10 px-6 py-10">
        <section id="about" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell artists about your venue..."
            testId="about"
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

        {children}
      </div>
    </div>
  );
}
