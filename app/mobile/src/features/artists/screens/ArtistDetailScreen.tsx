import { View, Text } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { Screen } from "../../../components/ui/Screen";
import type { ConcertNavParamList } from "../../../navigation/types";

type Props = NativeStackScreenProps<ConcertNavParamList, "ArtistDetail">;

export function ArtistDetailScreen({ route }: Props) {
  const { artistId } = route.params;

  return (
    <Screen>
      <View className="flex-1 items-center justify-center gap-4 px-6">
        <Text className="text-2xl font-bold text-gray-900">Artist</Text>
        <Text className="text-gray-500 text-center">Artist profile coming soon.</Text>
        <Text className="text-xs text-gray-400 font-mono">id: {artistId}</Text>
      </View>
    </Screen>
  );
}
