import { Hero } from "@/components/Hero";
import type { ImageFile } from "@concertable/shared";
import type { Artist } from "../types";

interface Props {
  artist: Artist;
  onNameChange?: (value: string) => void;
  onBannerChange?: (file: ImageFile) => void;
  onAvatarChange?: (file: ImageFile) => void;
}

export function ArtistHero({
  artist,
  onNameChange,
  onBannerChange,
  onAvatarChange,
}: Readonly<Props>) {
  return (
    <Hero
      bannerUrl={artist.bannerUrl}
      avatar={artist.avatar}
      name={artist.name}
      town={artist.town}
      county={artist.county}
      namePlaceholder="Artist name"
      onNameChange={onNameChange}
      onBannerChange={onBannerChange}
      onAvatarChange={onAvatarChange}
    />
  );
}
