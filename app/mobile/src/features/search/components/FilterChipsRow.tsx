import { useMemo } from "react";
import { Pressable, ScrollView } from "react-native";
import { MapPin, X } from "lucide-react-native";
import dayjs from "dayjs";
import { useGenresQuery, useSearchFiltersStore } from "@concertable/shared/features/search";
import { Text } from "@/components/ui/text";
import { theme } from "../../../lib/theme";
import { HEADER_TYPE_OPTIONS } from "../constants";

export function FilterChipsRow() {
  const { filters, setFilters } = useSearchFiltersStore();
  const { data: genres } = useGenresQuery();

  const chips = useMemo(() => {
    const result: { key: string; label: string; icon?: boolean; onRemove: () => void }[] = [];

    if (filters.headerType !== "concert") {
      const opt = HEADER_TYPE_OPTIONS.find((o) => o.value === filters.headerType);
      if (opt)
        result.push({
          key: "type",
          label: opt.label,
          onRemove: () => setFilters({ ...filters, headerType: "concert" }),
        });
    }

    if (filters.genreIds) {
      const ids = Array.isArray(filters.genreIds) ? filters.genreIds : [filters.genreIds as number];
      if (ids.length) {
        const names = (genres ?? []).filter((g) => ids.includes(g.id)).map((g) => g.name);
        if (names.length)
          result.push({
            key: "genres",
            label: names.join(", "),
            onRemove: () => setFilters({ ...filters, genreIds: undefined }),
          });
      }
    }

    if (filters.from || filters.to) {
      const from = filters.from ? dayjs(filters.from).format("D MMM") : "…";
      const to = filters.to ? dayjs(filters.to).format("D MMM") : "…";
      result.push({
        key: "dates",
        label: `${from} – ${to}`,
        onRemove: () => setFilters({ ...filters, from: undefined, to: undefined }),
      });
    }

    if (filters.lat != null) {
      const r = filters.radius ?? 25;
      const place = filters.locationLabel ?? "me";
      result.push({
        key: "location",
        label: `${r}km of ${place}`,
        icon: true,
        onRemove: () =>
          setFilters({
            ...filters,
            lat: undefined,
            lng: undefined,
            locationLabel: undefined,
            radius: undefined,
          }),
      });
    }

    return result;
  }, [filters, genres, setFilters]);

  if (!chips.length) return null;

  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      className="px-4 pb-3"
      contentContainerStyle={{ gap: 8 }}
    >
      {chips.map((chip) => (
        <Pressable
          key={chip.key}
          onPress={chip.onRemove}
          className="flex-row items-center gap-1.5 bg-primary/15 rounded-full px-3 py-1.5"
        >
          {chip.icon && <MapPin size={11} color={theme.primary} />}
          <Text className="text-xs font-medium text-primary">{chip.label}</Text>
          <X size={11} color={theme.primary} />
        </Pressable>
      ))}
    </ScrollView>
  );
}
