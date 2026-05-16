import { Pressable, View } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { Image } from "expo-image";
import { MapPin } from "lucide-react-native";
import type { ArtistHeader, Header } from "@concertable/shared/features/search";
import { useImageUrl } from "@concertable/shared/hooks";
import { RatingStars } from "@/components/ui/RatingStars";
import { GenreChips } from "@/components/ui/GenreChips";
import { Text } from "@/components/ui/text";
import { theme } from "../../../../lib/theme";
import type { SearchStackParamList } from "../../../../navigation/types";

type SearchNav = NativeStackNavigationProp<SearchStackParamList>;

export function ArtistHeaderCard({ data }: { data: Header }) {
  const nav = useNavigation<SearchNav>();
  const artist = data as ArtistHeader;
  const { data: src } = useImageUrl(artist.imageUrl);

  return (
    <Pressable onPress={() => nav.navigate("ArtistDetail", { artistId: artist.id })} style={{ flex: 1 }}>
      <View className="bg-card rounded-2xl border border-border overflow-hidden items-center p-4 gap-2">
        <Image
          source={{ uri: src }}
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
