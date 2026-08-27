import { Hero } from "@/components/Hero";
import type { ImageFile } from "@concertable/shared";
import { useVenueStore } from "../store/useVenueStore";
import type { Venue } from "../types";

interface Props {
  venue: Venue;
  onNameChange?: (value: string) => void;
  onBannerChange?: (file: ImageFile) => void;
  onAvatarChange?: (file: ImageFile) => void;
}

export function VenueHero({
  venue,
  onNameChange,
  onBannerChange,
  onAvatarChange,
}: Readonly<Props>) {
  const setBanner = useVenueStore((s) => s.setBanner);
  const setAvatar = useVenueStore((s) => s.setAvatar);

  return (
    <Hero
      bannerUrl={venue.bannerUrl}
      avatar={venue.avatar}
      name={venue.name}
      town={venue.town}
      county={venue.county}
      namePlaceholder="Venue name"
      onNameChange={onNameChange}
      onBannerChange={onBannerChange ?? setBanner}
      onAvatarChange={onAvatarChange ?? setAvatar}
    />
  );
}
