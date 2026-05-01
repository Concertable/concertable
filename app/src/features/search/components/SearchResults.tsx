import type { ComponentType } from "react";
import { useSearch } from "@tanstack/react-router";
import { HeaderCardSkeleton } from "@/components/skeletons/HeaderCardSkeleton";
import { useHeaderQuery } from "../hooks/useHeaderQuery";
import { ArtistHeaderCard } from "./headers/ArtistHeaderCard";
import { VenueHeaderCard } from "./headers/VenueHeaderCard";
import { ConcertHeaderCard } from "./headers/ConcertHeaderCard";
import type { Header, HeaderType } from "../types";
import type { SearchFilters } from "../schemas/searchSchema";

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
