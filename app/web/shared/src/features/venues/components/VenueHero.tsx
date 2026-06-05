import { Hero } from "@/components/Hero";
import { useVenueStore } from "../store/useVenueStore";
import type { Venue } from "../types";

interface Props {
  venue: Venue;
  onNameChange?: (value: string) => void;
}

export function VenueHero({ venue, onNameChange }: Readonly<Props>) {
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
      onBannerChange={setBanner}
      onAvatarChange={setAvatar}
    />
  );
}
