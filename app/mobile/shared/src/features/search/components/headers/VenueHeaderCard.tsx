import { Pressable, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { Image } from "expo-image";
import { MapPin } from "lucide-react-native";
import type { Header, VenueHeader } from "@concertable/shared/features/search";
import { useImageUrl } from "@concertable/shared/hooks";
import { RatingStars } from "@/components/ui/RatingStars";
import { Text } from "@/components/ui/text";
import { theme } from "../../../../lib/theme";
import type { SearchStackParamList } from "../../../../navigation/types";

type SearchNav = NativeStackNavigationProp<SearchStackParamList>;

export function VenueHeaderCard({ data }: { data: Header }) {
  const nav = useNavigation<SearchNav>();
  const venue = data as VenueHeader;
  const { data: src } = useImageUrl(venue.imageUrl);

  return (
    <Pressable onPress={() => nav.navigate("VenueDetail", { venueId: venue.id })} style={{ flex: 1 }}>
      <View className="bg-card rounded-2xl border border-border overflow-hidden">
        <Image
          source={{ uri: src }}
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
