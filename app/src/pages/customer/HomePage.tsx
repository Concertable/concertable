import { ConcertHeaderCarousel } from "@/components/headers/ConcertHeaderCarousel";
import { ArtistHeaderCarousel } from "@/components/headers/ArtistHeaderCarousel";
import { VenueHeaderCarousel } from "@/components/headers/VenueHeaderCarousel";

export default function HomePage() {
  return (
    <div className="max-w-6xl mx-auto px-6 py-10 space-y-10">
      <ConcertHeaderCarousel title="Discover Concerts" />
      <div className="border-t border-border" />
      <ArtistHeaderCarousel title="Discover Artists" />
      <div className="border-t border-border" />
      <VenueHeaderCarousel title="Discover Venues" />
    </div>
  );
}
