import { AboutSection } from "@/components/details/AboutSection";
import { LocationSection } from "@/components/details/LocationSection";
import type { DetailsSection } from "@/components/details/DetailsLayout";
import { ReviewSection } from "@/features/reviews";
import { VenueConcerts } from "./components/VenueConcerts";
import type { Venue } from "./types";

export interface VenueEdits {
  onAboutChange?: (value: string) => void;
}

export function venueSections(venue: Venue, edits?: VenueEdits) {
  const about: DetailsSection = {
    id: "about",
    label: "About",
    content: (
      <AboutSection
        text={venue.about}
        placeholder="Tell artists about your venue..."
        onChange={edits?.onAboutChange}
      />
    ),
  };

  const location: DetailsSection = {
    id: "location",
    label: "Location",
    content: (
      <LocationSection
        town={venue.town}
        county={venue.county}
        lat={venue.latitude}
        lng={venue.longitude}
      />
    ),
  };

  const concerts: DetailsSection = {
    id: "concerts",
    label: "Concerts",
    content: <VenueConcerts />,
  };

  const reviews: DetailsSection = {
    id: "reviews",
    label: "Reviews",
    content: <ReviewSection type="venue" id={venue.id} />,
  };

  return { about, location, concerts, reviews };
}
