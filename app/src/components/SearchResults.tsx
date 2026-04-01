import type { ComponentType } from "react";
import type { Header, HeaderType } from "@/types/header";
import { useSearchFilters } from "@/hooks/useSearchFilters";
import { useHeaderQuery } from "@/hooks/query/useHeaderQuery";
import { ArtistHeaderCard } from "@/components/headers/ArtistHeaderCard";
import { VenueHeaderCard } from "@/components/headers/VenueHeaderCard";
import { ConcertHeaderCard } from "@/components/headers/ConcertHeaderCard";

const cardRegistry = {
  artist: ArtistHeaderCard,
  venue: VenueHeaderCard,
  concert: ConcertHeaderCard,
} as Record<HeaderType, ComponentType<{ data: Header }>>;

export function SearchResults() {
  const { filters } = useSearchFilters();
  const { data, isLoading, isError } = useHeaderQuery(filters);

  if (isLoading) return <p className="text-muted-foreground text-sm">Loading...</p>;
  if (isError) return <p className="text-destructive text-sm">Something went wrong.</p>;
  if (!data?.data.length) return <p className="text-muted-foreground text-sm">No results found.</p>;

  const Card = cardRegistry[filters.headerType];

  return (
    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      {data.data.map((header: Header) => (
        <Card key={header.id} data={header} />
      ))}
    </div>
  );
}
