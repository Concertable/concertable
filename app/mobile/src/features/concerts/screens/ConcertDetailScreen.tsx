import { ScrollView, Image, Text, View, ActivityIndicator } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { useConcert } from "@concertable/shared/features/concerts";
import { Badge } from "../../../components/ui/Badge";
import { Button } from "../../../components/ui/Button";
import dayjs from "dayjs";
import type { ConcertNavParamList } from "../../../navigation/types";

type Props = NativeStackScreenProps<ConcertNavParamList, "ConcertDetail">;

export function ConcertDetailScreen({ navigation, route }: Props) {
  const { concertId } = route.params;
  const { concert, isLoading, isError } = useConcert(concertId);

  if (isLoading)
    return (
      <View className="flex-1 items-center justify-center bg-white">
        <ActivityIndicator size="large" />
      </View>
    );

  if (isError || !concert)
    return (
      <View className="flex-1 items-center justify-center bg-white">
        <Text className="text-red-500">Failed to load concert.</Text>
      </View>
    );

  const soldOut = concert.availableTickets === 0;

  return (
    <ScrollView className="flex-1 bg-white">
      {concert.bannerUrl ? (
        <Image source={{ uri: concert.bannerUrl }} className="w-full h-52" resizeMode="cover" />
      ) : (
        <View className="w-full h-52 bg-gray-100" />
      )}

      <View className="p-4 gap-4">
        <View className="gap-1">
          <Text className="text-2xl font-bold text-gray-900">{concert.name}</Text>
          <Text className="text-gray-500">
            {dayjs(concert.startDate).format("ddd D MMM YYYY")} — {dayjs(concert.endDate).format("ddd D MMM YYYY")}
          </Text>
        </View>

        <View className="flex-row gap-3">
          <View className="flex-1 bg-gray-50 rounded-xl p-3 gap-0.5">
            <Text className="text-xs text-gray-500">Venue</Text>
            <Text className="font-semibold text-gray-900">{concert.venue.name}</Text>
            <Text className="text-xs text-gray-400">{concert.venue.town}, {concert.venue.county}</Text>
          </View>
          <View className="flex-1 bg-gray-50 rounded-xl p-3 gap-0.5">
            <Text className="text-xs text-gray-500">Artist</Text>
            <Text className="font-semibold text-gray-900">{concert.artist.name}</Text>
            <Text className="text-xs text-gray-400">{concert.artist.town}, {concert.artist.county}</Text>
          </View>
        </View>

        {concert.genres.length > 0 && (
          <View className="flex-row flex-wrap gap-2">
            {concert.genres.map((g) => (
              <Badge key={g.id} variant="secondary">{g.name}</Badge>
            ))}
          </View>
        )}

        {concert.about ? (
          <Text className="text-gray-700 leading-relaxed">{concert.about}</Text>
        ) : null}

        <View className="flex-row items-center justify-between py-2">
          <View>
            <Text className="text-xs text-gray-500">Ticket price</Text>
            <Text className="text-2xl font-bold text-gray-900">£{concert.price.toFixed(2)}</Text>
          </View>
          <Text className="text-sm text-gray-500">
            {soldOut ? "Sold out" : `${concert.availableTickets} left`}
          </Text>
        </View>

        <Button
          disabled={soldOut}
          onPress={() => navigation.navigate("TicketCheckout", { concertId: concert.id })}
        >
          {soldOut ? "Sold Out" : "Buy Ticket"}
        </Button>
      </View>
    </ScrollView>
  );
}
