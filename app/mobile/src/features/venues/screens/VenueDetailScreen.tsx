import { View, Text } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { Screen } from "../../../components/ui/Screen";
import type { ConcertNavParamList } from "../../../navigation/types";

type Props = NativeStackScreenProps<ConcertNavParamList, "VenueDetail">;

export function VenueDetailScreen({ route }: Props) {
  const { venueId } = route.params;

  return (
    <Screen>
      <View className="flex-1 items-center justify-center gap-4 px-6">
        <Text className="text-2xl font-bold text-gray-900">Venue</Text>
        <Text className="text-gray-500 text-center">Venue profile coming soon.</Text>
        <Text className="text-xs text-gray-400 font-mono">id: {venueId}</Text>
      </View>
    </Screen>
  );
}
