import { MapPin } from "lucide-react";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ReviewSection } from "@/features/reviews";
import { ScrollspyNav } from "@/components/ScrollspyNav";
import { useArtistStore } from "../store/useArtistStore";
import type { Artist } from "../types";

const SECTIONS = [
  { id: "about", label: "About" },
  { id: "location", label: "Location" },
  { id: "concerts", label: "Concerts" },
  { id: "opportunities", label: "Opportunities" },
  { id: "reviews", label: "Reviews" },
];

interface Props {
  artist: Artist;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
}

export function ArtistDetails({
  artist,
  onNameChange,
  onAboutChange,
}: Readonly<Props>) {
  const setBanner = useArtistStore((s) => s.setBanner);
  const setAvatar = useArtistStore((s) => s.setAvatar);

  return (
    <div>
      <Hero
        bannerUrl={artist.bannerUrl}
        avatar={artist.avatar}
        name={artist.name}
        town={artist.town}
        county={artist.county}
        namePlaceholder="Artist name"
        onNameChange={onNameChange}
        onBannerChange={setBanner}
        onAvatarChange={setAvatar}
      />

      <ScrollspyNav sections={SECTIONS} />

      <div className="mx-auto max-w-4xl space-y-10 px-6 py-10">
        <section id="about" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell venues about yourself..."
          >
            {artist.about}
          </EditableTextarea>
        </section>

        <div className="border-border border-t" />

        <section id="location" className="scroll-mt-24 space-y-3">
          <h2 className="text-xl font-semibold">Location</h2>
          <p className="text-muted-foreground flex items-center gap-2">
            <MapPin className="size-4" />
            {[artist.town, artist.county].filter(Boolean).join(", ") ||
              "No location set."}
          </p>
        </section>

        <div className="border-border border-t" />

        <section id="concerts" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">Concerts</h2>
          <p className="text-muted-foreground">No upcoming concerts.</p>
        </section>

        <div className="border-border border-t" />

        <section id="opportunities" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">Opportunities</h2>
          <p className="text-muted-foreground">No opportunities yet.</p>
        </section>

        <div className="border-border border-t" />

        <section id="reviews" className="scroll-mt-24">
          <ReviewSection type="artist" id={artist.id} />
        </section>
      </div>
    </div>
  );
}
