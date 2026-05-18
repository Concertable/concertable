import { ReactNode } from "react";
import { MapPin } from "lucide-react";
import { Hero } from "@/components/Hero";
import { EditableTextarea } from "@/components/editable/EditableTextarea";
import { ScrollspyNav } from "@/components/ScrollspyNav";
import { useArtistStore } from "../store/useArtistStore";
import type { Artist } from "../types";

interface Section {
  id: string;
  label: string;
}

interface Props {
  artist: Artist;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
  sections?: Section[];
  children?: ReactNode;
}

export function ArtistDetails({
  artist,
  onNameChange,
  onAboutChange,
  sections,
  children,
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

      {sections && <ScrollspyNav sections={sections} />}

      <div className="mx-auto max-w-4xl space-y-10 px-6 py-10">
        <section id="about" className="scroll-mt-24 space-y-2">
          <h2 className="text-xl font-semibold">About</h2>
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell venues about yourself..."
            testId="about"
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

        {children}
      </div>
    </div>
  );
}
