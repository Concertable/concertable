import { Keyboard, Pressable, TextInput, View } from "react-native";
import { Search, SlidersHorizontal, X } from "lucide-react-native";
import { useSearchFiltersStore } from "@concertable/shared/features/search";
import { theme } from "../../../lib/theme";

interface Props {
  focused: boolean;
  onFocusChange: (focused: boolean) => void;
  onOpenFilters: () => void;
}

export function SearchBar({ focused, onFocusChange, onOpenFilters }: Props) {
  const { filters, setFilters } = useSearchFiltersStore();

  const hasActiveFilters =
    filters.headerType !== "concert" ||
    !!(filters.genreIds && (Array.isArray(filters.genreIds) ? filters.genreIds.length : true)) ||
    !!(filters.from || filters.to) ||
    filters.lat != null ||
    !!filters.orderBy;

  return (
    <View style={{ flexDirection: "row", alignItems: "center", paddingHorizontal: 16, paddingTop: 12, paddingBottom: 8 }}>
      <View
        style={{
          flex: 1,
          flexDirection: "row",
          alignItems: "center",
          backgroundColor: theme.muted,
          borderRadius: 9999,
          paddingLeft: 16,
          paddingRight: 6,
          height: 44,
        }}
      >
        <Search size={16} color={theme.mutedForeground} />
        <TextInput
          style={{ flex: 1, fontSize: 14, color: theme.foreground, marginLeft: 8 }}
          placeholder="Search concerts, artists, venues…"
          placeholderTextColor={theme.mutedForeground}
          value={filters.query ?? ""}
          onChangeText={(text) => setFilters({ ...filters, query: text || undefined })}
          onFocus={() => onFocusChange(true)}
          onBlur={() => onFocusChange(false)}
          returnKeyType="search"
          autoCapitalize="none"
        />
        {!!filters.query && (
          <Pressable onPress={() => setFilters({ ...filters, query: undefined })} style={{ padding: 4 }}>
            <X size={15} color={theme.mutedForeground} />
          </Pressable>
        )}
        <Pressable
          onPress={() => { Keyboard.dismiss(); onFocusChange(false); }}
          style={{ width: 32, height: 32, borderRadius: 16, backgroundColor: theme.primary, alignItems: "center", justifyContent: "center", marginLeft: 4 }}
        >
          <Search size={14} color={theme.primaryForeground} />
        </Pressable>
      </View>

      <Pressable onPress={onOpenFilters} style={{ marginLeft: 12 }}>
        <View>
          <SlidersHorizontal size={20} color={hasActiveFilters ? theme.primary : theme.mutedForeground} />
          {hasActiveFilters && (
            <View style={{ position: "absolute", top: -2, right: -2, width: 7, height: 7, borderRadius: 4, backgroundColor: theme.primary }} />
          )}
        </View>
      </Pressable>
    </View>
  );
}
