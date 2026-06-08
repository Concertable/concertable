import { AboutSection } from "@/components/details/AboutSection";
import { LocationSection } from "@/components/details/LocationSection";
import type { DetailsSection } from "@/components/details/DetailsLayout";
import { ReviewSection } from "@/features/reviews";
import { ArtistConcerts } from "./components/ArtistConcerts";
import type { Artist } from "./types";

export interface ArtistEdits {
  onAboutChange?: (value: string) => void;
}

export function artistSections(artist: Artist, edits?: ArtistEdits) {
  const about: DetailsSection = {
    id: "about",
    label: "About",
    content: (
      <AboutSection
        text={artist.about}
        placeholder="Tell venues about yourself..."
        onChange={edits?.onAboutChange}
      />
    ),
  };

  const location: DetailsSection = {
    id: "location",
    label: "Location",
    content: <LocationSection town={artist.town} county={artist.county} />,
  };

  const concerts: DetailsSection = {
    id: "concerts",
    label: "Concerts",
    content: <ArtistConcerts />,
  };

  const reviews: DetailsSection = {
    id: "reviews",
    label: "Reviews",
    content: <ReviewSection type="artist" id={artist.id} />,
  };

  return { about, location, concerts, reviews };
}
