import { Pressable, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { Image } from "expo-image";
import { MapPin } from "lucide-react-native";
import dayjs from "dayjs";
import type { ConcertHeader, Header } from "@concertable/shared/features/search";
import { useImageUrl } from "@concertable/shared/hooks";
import { RatingStars } from "../../../../components/ui/RatingStars";
import { GenreChips } from "../../../../components/ui/GenreChips";
import { Text } from "@/components/ui/text";
import { theme } from "../../../../lib/theme";
import type { SearchStackParamList } from "../../../../navigation/types";

type SearchNav = NativeStackNavigationProp<SearchStackParamList>;

export function ConcertHeaderCard({ data }: { data: Header }) {
  const nav = useNavigation<SearchNav>();
  const concert = data as ConcertHeader;
  const { data: src } = useImageUrl(concert.imageUrl);

  return (
    <Pressable onPress={() => nav.navigate("ConcertDetail", { concertId: concert.id })} style={{ flex: 1 }}>
      <View className="bg-card rounded-2xl border border-border overflow-hidden">
        <Image
          source={{ uri: src }}
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
