import { useMemo, useRef, useState } from "react";
import { FlatList, Keyboard, Pressable, ScrollView, Text, TextInput, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import type { BottomSheetModal } from "@gorhom/bottom-sheet";
import { Image } from "expo-image";
import { MapPin, Search, SlidersHorizontal, X } from "lucide-react-native";
import {
  useAutocompleteQuery,
  useGenresQuery,
  useHeaderQuery,
  useSearchFiltersStore,
} from "@concertable/shared/features/search";
import type {
  ArtistHeader,
  AutocompleteResult,
  ConcertHeader,
  Header,
  VenueHeader,
} from "@concertable/shared/features/search";
import { Screen } from "../../../components/ui/Screen";
import { SegmentedControl } from "../../../components/ui/SegmentedControl";
import { RatingStars } from "../../../components/ui/RatingStars";
import { GenreChips } from "../../../components/ui/GenreChips";
import { EmptyState } from "../../../components/ui/EmptyState";
import { Skeleton } from "../../../components/ui/Skeleton";
import { SearchFilterSheet } from "./SearchFilterSheet";
import { theme } from "../../../lib/theme";
import dayjs from "dayjs";
import type { SearchStackParamList } from "../../../navigation/types";

const HEADER_TYPE_OPTIONS = [
  { value: "concert" as const, label: "Concerts" },
  { value: "artist" as const, label: "Artists" },
  { value: "venue" as const, label: "Venues" },
];

type SearchNav = NativeStackNavigationProp<SearchStackParamList>;

export function SearchScreen() {
  const nav = useNavigation<SearchNav>();
  const { filters, setFilters } = useSearchFiltersStore();
  const [focused, setFocused] = useState(false);
  const filterSheetRef = useRef<BottomSheetModal>(null);

  const { data: results, isFetching } = useHeaderQuery(filters);
  const { data: autocomplete } = useAutocompleteQuery(filters.query ?? "", filters.headerType);
  const { data: genres } = useGenresQuery();

  const showAutocomplete = focused && !!(filters.query) && !!(autocomplete?.length);

  const filterChips = useMemo(() => {
    const chips: { key: string; label: string; onRemove: () => void }[] = [];

    if (filters.genreIds) {
      const ids = Array.isArray(filters.genreIds) ? filters.genreIds : [filters.genreIds as number];
      if (ids.length) {
        const names = (genres ?? []).filter((g) => ids.includes(g.id)).map((g) => g.name);
        if (names.length) {
          chips.push({
            key: "genres",
            label: names.join(", "),
            onRemove: () => setFilters({ ...filters, genreIds: undefined }),
          });
        }
      }
    }

    if (filters.from || filters.to) {
      const from = filters.from ? dayjs(filters.from).format("D MMM") : "…";
      const to = filters.to ? dayjs(filters.to).format("D MMM") : "…";
      chips.push({
        key: "dates",
        label: `${from} – ${to}`,
        onRemove: () => setFilters({ ...filters, from: undefined, to: undefined }),
      });
    }

    if (filters.lat != null) {
      const r = filters.radius ?? 25;
      chips.push({
        key: "location",
        label: `Within ${r}km`,
        onRemove: () => setFilters({ ...filters, lat: undefined, lng: undefined, radius: undefined }),
      });
    }

    return chips;
  }, [filters, genres, setFilters]);

  function handleAutocompleteSelect(item: AutocompleteResult) {
    Keyboard.dismiss();
    if (item.$type === "concert") nav.navigate("ConcertDetail", { concertId: item.id });
    else if (item.$type === "artist") nav.navigate("ArtistDetail", { artistId: item.id });
    else nav.navigate("VenueDetail", { venueId: item.id });
  }

  function renderGridItem({ item }: { item: Header }) {
    const ht = filters.headerType;
    if (ht === "concert") {
      const c = item as ConcertHeader;
      return <GridConcertCard concert={c} onPress={() => nav.navigate("ConcertDetail", { concertId: c.id })} />;
    }
    if (ht === "artist") {
      const a = item as ArtistHeader;
      return <GridArtistCard artist={a} onPress={() => nav.navigate("ArtistDetail", { artistId: a.id })} />;
    }
    const v = item as VenueHeader;
    return <GridVenueCard venue={v} onPress={() => nav.navigate("VenueDetail", { venueId: v.id })} />;
  }

  return (
    <Screen padded={false}>
      <View className="border-b border-border">
        <View className="px-4 pt-3 pb-2">
          <View className="flex-row items-center bg-muted rounded-full px-4 h-11 gap-2">
            <Search size={16} color={theme.mutedForeground} />
            <TextInput
              className="flex-1 text-sm text-foreground"
              placeholder="Search concerts, artists, venues…"
              placeholderTextColor={theme.mutedForeground}
              value={filters.query ?? ""}
              onChangeText={(text) => setFilters({ ...filters, query: text || undefined })}
              onFocus={() => setFocused(true)}
              onBlur={() => setFocused(false)}
              returnKeyType="search"
              autoCapitalize="none"
            />
            {filters.query ? (
              <Pressable onPress={() => setFilters({ ...filters, query: undefined })}>
                <X size={16} color={theme.mutedForeground} />
              </Pressable>
            ) : (
              <Pressable onPress={() => filterSheetRef.current?.present()}>
                <SlidersHorizontal
                  size={18}
                  color={filterChips.length > 0 ? theme.primary : theme.mutedForeground}
                />
              </Pressable>
            )}
          </View>
        </View>

        <View className="px-4 pb-3">
          <SegmentedControl
            options={HEADER_TYPE_OPTIONS}
            value={filters.headerType}
            onChange={(v) => setFilters({ ...filters, headerType: v })}
          />
        </View>

        {filterChips.length > 0 && (
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            className="px-4 pb-3"
            contentContainerStyle={{ gap: 8 }}
          >
            {filterChips.map((chip) => (
              <Pressable
                key={chip.key}
                onPress={chip.onRemove}
                className="flex-row items-center gap-1.5 bg-primary/15 rounded-full px-3 py-1.5"
              >
                <Text className="text-xs font-medium text-primary">{chip.label}</Text>
                <X size={11} color={theme.primary} />
              </Pressable>
            ))}
          </ScrollView>
        )}
      </View>

      {showAutocomplete ? (
        <FlatList
          data={autocomplete}
          keyExtractor={(item) => `${item.$type}-${item.id}`}
          showsVerticalScrollIndicator={false}
          renderItem={({ item }) => (
            <Pressable
              onPress={() => handleAutocompleteSelect(item)}
              className="flex-row items-center justify-between px-4 py-3.5 border-b border-border"
            >
              <Text className="text-sm text-foreground flex-1" numberOfLines={1}>
                {item.name}
              </Text>
              <Text className="text-xs text-muted-foreground capitalize ml-3">{item.$type}</Text>
            </Pressable>
          )}
        />
      ) : (
        <FlatList
          key={filters.headerType}
          data={results?.data ?? []}
          keyExtractor={(item) => `${item.$type}-${item.id}`}
          numColumns={2}
          columnWrapperStyle={{ gap: 12 }}
          contentContainerStyle={{ padding: 16, gap: 12 }}
          showsVerticalScrollIndicator={false}
          renderItem={renderGridItem}
          ListEmptyComponent={
            isFetching ? (
              <View className="flex-row flex-wrap gap-3">
                {[0, 1, 2, 3].map((i) => (
                  <Skeleton key={i} width={160} height={220} className="rounded-2xl" />
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
      )}

      <SearchFilterSheet ref={filterSheetRef} />
    </Screen>
  );
}

interface GridConcertCardProps {
  concert: ConcertHeader;
  onPress: () => void;
}

function GridConcertCard({ concert, onPress }: Readonly<GridConcertCardProps>) {
  return (
    <Pressable onPress={onPress} className="flex-1">
      <View className="bg-card rounded-2xl border border-border overflow-hidden">
        <Image
          source={{ uri: concert.imageUrl }}
          style={{ width: "100%", height: 120 }}
          contentFit="cover"
          className="bg-muted"
        />
        <View className="p-3 gap-1">
          <Text className="text-sm font-semibold text-foreground" numberOfLines={1}>
            {concert.name}
          </Text>
          <Text className="text-xs text-muted-foreground">
            {dayjs(concert.startDate).format("D MMM YYYY")}
          </Text>
          <View className="flex-row items-center gap-1">
            <MapPin size={10} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground" numberOfLines={1}>
              {concert.town}
            </Text>
          </View>
          {concert.rating != null && <RatingStars rating={concert.rating} size={12} />}
          {concert.genres?.length > 0 && (
            <GenreChips genres={concert.genres.slice(0, 2)} className="mt-0.5" />
          )}
        </View>
      </View>
    </Pressable>
  );
}

interface GridArtistCardProps {
  artist: ArtistHeader;
  onPress: () => void;
}

function GridArtistCard({ artist, onPress }: Readonly<GridArtistCardProps>) {
  return (
    <Pressable onPress={onPress} className="flex-1">
      <View className="bg-card rounded-2xl border border-border overflow-hidden items-center p-4 gap-2">
        <Image
          source={{ uri: artist.imageUrl }}
          style={{ width: 72, height: 72, borderRadius: 36 }}
          contentFit="cover"
          className="bg-muted"
        />
        <Text className="text-sm font-semibold text-foreground text-center" numberOfLines={1}>
          {artist.name}
        </Text>
        <View className="flex-row items-center gap-1">
          <MapPin size={10} color={theme.mutedForeground} />
          <Text className="text-xs text-muted-foreground" numberOfLines={1}>
            {artist.town}
          </Text>
        </View>
        {artist.rating != null && <RatingStars rating={artist.rating} size={12} />}
        {artist.genres?.length > 0 && <GenreChips genres={artist.genres.slice(0, 2)} />}
      </View>
    </Pressable>
  );
}

interface GridVenueCardProps {
  venue: VenueHeader;
  onPress: () => void;
}

function GridVenueCard({ venue, onPress }: Readonly<GridVenueCardProps>) {
  return (
    <Pressable onPress={onPress} className="flex-1">
      <View className="bg-card rounded-2xl border border-border overflow-hidden">
        <Image
          source={{ uri: venue.imageUrl }}
          style={{ width: "100%", height: 100 }}
          contentFit="cover"
          className="bg-muted"
        />
        <View className="p-3 gap-1">
          <Text className="text-sm font-semibold text-foreground" numberOfLines={1}>
            {venue.name}
          </Text>
          <View className="flex-row items-center gap-1">
            <MapPin size={10} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground" numberOfLines={1}>
              {venue.town}
            </Text>
          </View>
          {venue.rating != null && <RatingStars rating={venue.rating} size={12} />}
        </View>
      </View>
    </Pressable>
  );
}
