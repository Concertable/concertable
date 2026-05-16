import { FlatList, Pressable } from "react-native";
import { useAutocompleteQuery } from "@concertable/shared/features/search";
import type { AutocompleteResult, HeaderType } from "@concertable/shared/features/search";
import { Text } from "@/components/ui/text";

interface Props {
  query: string;
  headerType: HeaderType;
  onSelect: (item: AutocompleteResult) => void;
}

export function AutocompleteList({ query, headerType, onSelect }: Props) {
  const { data: autocomplete } = useAutocompleteQuery(query, headerType);

  return (
    <FlatList
      style={{ flex: 1 }}
      data={autocomplete ?? []}
      keyExtractor={(item) => `${item.$type}-${item.id}`}
      showsVerticalScrollIndicator={false}
      renderItem={({ item }) => (
        <Pressable
          onPress={() => onSelect(item)}
          className="flex-row items-center justify-between px-4 py-3.5 border-b border-border"
        >
          <Text className="text-sm text-foreground flex-1" numberOfLines={1}>
            {item.name}
          </Text>
          <Text className="text-xs text-muted-foreground capitalize ml-3">{item.$type}</Text>
        </Pressable>
      )}
    />
  );
}
