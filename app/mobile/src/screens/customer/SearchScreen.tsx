import { useState, useCallback } from "react";
import { FlatList, Image, Pressable, Text, View, ActivityIndicator, TextInput } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import {
  useHeaderQuery,
  useSearchFiltersStore,
} from "@concertable/shared/features/search";
import type { ConcertHeader } from "@concertable/shared/features/search";
import { Screen } from "../../components/ui/Screen";
import { Card } from "../../components/ui/Card";
import { Badge } from "../../components/ui/Badge";
import dayjs from "dayjs";
import type { SearchStackParamList } from "../../navigation/types";

type Props = NativeStackScreenProps<SearchStackParamList, "SearchMain">;

export function SearchScreen({ navigation }: Props) {
  const { filters, setFilters } = useSearchFiltersStore();
  const [query, setQuery] = useState(filters.query ?? "");

  const { data: results, isFetching } = useHeaderQuery({
    ...filters,
    headerType: "concert",
    query: query || undefined,
  });

  const onChangeText = useCallback(
    (text: string) => {
      setQuery(text);
      setFilters({ ...filters, headerType: "concert", query: text || undefined });
    },
    [filters, setFilters],
  );

  return (
    <Screen>
      <View className="bg-gray-100 rounded-xl flex-row items-center px-3 mb-4">
        <TextInput
          className="flex-1 py-3 text-base text-gray-900"
          placeholder="Search concerts..."
          placeholderTextColor="#9ca3af"
          value={query}
          onChangeText={onChangeText}
          returnKeyType="search"
          autoCapitalize="none"
        />
        {isFetching && <ActivityIndicator size="small" className="ml-2" />}
      </View>

      <FlatList
        data={results?.data ?? []}
        keyExtractor={(item) => String(item.id)}
        showsVerticalScrollIndicator={false}
        contentContainerClassName="gap-3 pb-4"
        renderItem={({ item }) => (
          <SearchResultCard
            concert={item as ConcertHeader}
            onPress={() => navigation.navigate("ConcertDetail", { concertId: item.id })}
          />
        )}
        ListEmptyComponent={
          !isFetching ? (
            <Text className="text-gray-500 text-center mt-12">
              {query ? "No concerts found." : "Start typing to search concerts."}
            </Text>
          ) : null
        }
      />
    </Screen>
  );
}

function SearchResultCard({ concert, onPress }: { concert: ConcertHeader; onPress: () => void }) {
  return (
    <Pressable onPress={onPress}>
      <Card className="flex-row gap-3 items-center">
        {concert.imageUrl ? (
          <Image
            source={{ uri: concert.imageUrl }}
            style={{ width: 64, height: 64, borderRadius: 8 }}
            resizeMode="cover"
          />
        ) : (
          <View style={{ width: 64, height: 64, borderRadius: 8 }} className="bg-gray-100" />
        )}
        <View className="flex-1 gap-1">
          <Text className="font-semibold text-gray-900" numberOfLines={1}>{concert.name}</Text>
          <Text className="text-sm text-gray-500">
            {dayjs(concert.startDate).format("D MMM")} — {dayjs(concert.endDate).format("D MMM YYYY")}
          </Text>
          <Text className="text-xs text-gray-400">{concert.town}, {concert.county}</Text>
          {concert.genres?.length > 0 && (
            <View className="flex-row flex-wrap gap-1 mt-0.5">
              {concert.genres.slice(0, 3).map((g) => (
                <Badge key={g.id} variant="secondary">{g.name}</Badge>
              ))}
            </View>
          )}
        </View>
      </Card>
    </Pressable>
  );
}
