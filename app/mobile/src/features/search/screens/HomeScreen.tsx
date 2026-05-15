import { FlatList, Image, Pressable, Text, View, ActivityIndicator } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { useHeaderAmountQuery } from "@concertable/shared/features/search";
import type { ConcertHeader } from "@concertable/shared/features/search";
import { Screen } from "../../../components/ui/Screen";
import { Card } from "../../../components/ui/Card";
import dayjs from "dayjs";
import type { HomeStackParamList } from "../../../navigation/types";

type Props = NativeStackScreenProps<HomeStackParamList, "HomeMain">;

export function HomeScreen({ navigation }: Props) {
  const { data: concerts, isLoading } = useHeaderAmountQuery("concert", 20);

  if (isLoading)
    return (
      <Screen>
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" />
        </View>
      </Screen>
    );

  return (
    <Screen>
      <Text className="text-2xl font-bold text-gray-900 mb-4">Upcoming Concerts</Text>
      <FlatList
        data={concerts}
        keyExtractor={(item) => String(item.id)}
        showsVerticalScrollIndicator={false}
        contentContainerClassName="gap-3 pb-4"
        renderItem={({ item }) => (
          <ConcertCard
            concert={item as ConcertHeader}
            onPress={() => navigation.navigate("ConcertDetail", { concertId: item.id })}
          />
        )}
        ListEmptyComponent={
          <Text className="text-gray-500 text-center mt-12">No concerts right now.</Text>
        }
      />
    </Screen>
  );
}

function ConcertCard({ concert, onPress }: { concert: ConcertHeader; onPress: () => void }) {
  return (
    <Pressable onPress={onPress}>
      <Card className="p-0 overflow-hidden">
        {concert.imageUrl ? (
          <Image source={{ uri: concert.imageUrl }} className="w-full h-40" resizeMode="cover" />
        ) : (
          <View className="w-full h-40 bg-gray-100" />
        )}
        <View className="p-3 gap-1">
          <Text className="text-base font-semibold text-gray-900" numberOfLines={1}>
            {concert.name}
          </Text>
          <Text className="text-sm text-gray-500">
            {dayjs(concert.startDate).format("D MMM YYYY")} — {dayjs(concert.endDate).format("D MMM YYYY")}
          </Text>
          <Text className="text-sm text-gray-500">{concert.town}, {concert.county}</Text>
        </View>
      </Card>
    </Pressable>
  );
}
