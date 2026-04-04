import { ConcertHeaderCarousel } from "@/components/headers/ConcertHeaderCarousel";
import { ArtistHeaderCarousel } from "@/components/headers/ArtistHeaderCarousel";
import { VenueHeaderCarousel } from "@/components/headers/VenueHeaderCarousel";

export default function HomePage() {
  return (
    <div className="mx-auto max-w-6xl space-y-10 px-6 py-10">
      <ConcertHeaderCarousel title="Discover Concerts" />
      <div className="border-border border-t" />
      <ArtistHeaderCarousel title="Discover Artists" />
      <div className="border-border border-t" />
      <VenueHeaderCarousel title="Discover Venues" />
    </div>
  );
}
