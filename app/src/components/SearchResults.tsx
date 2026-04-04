import type { ComponentType } from "react";
import type { Header, HeaderType } from "@/types/header";
import type { SearchFilters } from "@/schemas/searchSchema";
import { useSearch } from "@tanstack/react-router";
import { useHeaderQuery } from "@/hooks/query/useHeaderQuery";
import { ArtistHeaderCard } from "@/components/headers/ArtistHeaderCard";
import { VenueHeaderCard } from "@/components/headers/VenueHeaderCard";
import { ConcertHeaderCard } from "@/components/headers/ConcertHeaderCard";
import { HeaderCardSkeleton } from "@/components/skeletons/HeaderCardSkeleton";

const cardRegistry = {
  artist: ArtistHeaderCard,
  venue: VenueHeaderCard,
  concert: ConcertHeaderCard,
} as Record<HeaderType, ComponentType<{ data: Header }>>;

export function SearchResults() {
  const filters = useSearch({ strict: false }) as SearchFilters;
  const { data, isLoading, isError } = useHeaderQuery(filters);

  if (isLoading)
    return (
      <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <HeaderCardSkeleton key={i} />
        ))}
      </div>
    );
  if (isError)
    return <p className="text-destructive text-sm">Something went wrong.</p>;
  if (!data?.data.length)
    return <p className="text-muted-foreground text-sm">No results found.</p>;

  const Card = cardRegistry[filters.headerType];

  return (
    <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-4">
      {data.data.map((header: Header) => (
        <Card key={header.id} data={header} />
      ))}
    </div>
  );
}
