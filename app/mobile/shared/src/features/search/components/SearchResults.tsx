import type { ComponentType } from "react";
import { FlatList, View } from "react-native";
import { useHeaderQuery, useSearchFiltersStore } from "@concertable/shared/features/search";
import type { Header, HeaderType } from "@concertable/shared/features/search";
import { Skeleton } from "@/components/ui/skeleton";
import { EmptyState } from "@/components/ui/EmptyState";
import { ConcertHeaderCard } from "./headers/ConcertHeaderCard";
import { ArtistHeaderCard } from "./headers/ArtistHeaderCard";
import { VenueHeaderCard } from "./headers/VenueHeaderCard";
import { logger } from "../../../lib/logger";

const cardRegistry = {
  artist: ArtistHeaderCard,
  venue: VenueHeaderCard,
  concert: ConcertHeaderCard,
} as Record<HeaderType, ComponentType<{ data: Header }>>;

export function SearchResults() {
  const { filters } = useSearchFiltersStore();
  const { data: results, isFetching } = useHeaderQuery(filters);
  const Card = cardRegistry[filters.headerType];

  logger.log("[search] filters", filters);
  logger.log("[search] results", results?.data?.length ?? 0, "items, fetching:", isFetching);
  logger.log("[search] first item", results?.data?.[0]);

  return (
    <FlatList
      style={{ flex: 1 }}
      key={filters.headerType}
      data={results?.data ?? []}
      keyExtractor={(item) => `${item.$type}-${item.id}`}
      numColumns={2}
      columnWrapperStyle={{ gap: 12 }}
      contentContainerStyle={{ padding: 16, gap: 12 }}
      showsVerticalScrollIndicator={false}
      renderItem={({ item }) => <Card data={item} />}
      ListEmptyComponent={
        isFetching ? (
          <View className="flex-row flex-wrap gap-3">
            {[0, 1, 2, 3].map((i) => (
              <Skeleton key={i} className="w-40 h-[220px] rounded-2xl" />
            ))}
          </View>
        ) : (
          <EmptyState
            title={`No ${filters.headerType}s found`}
            description="Try adjusting your search or filters"
            className="mt-8"
          />
        )
      }
    />
  );
}
