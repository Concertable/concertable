import { View } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { useVenue } from "@concertable/shared/features/venues";
import { EditableProvider } from "@concertable/shared/providers";
import { Screen } from "@/components/ui/Screen";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/ui/ErrorState";
import { VenueDetails } from "../components/VenueDetails";
import type { ConcertNavParamList } from "../../../navigation/types";

type Props = NativeStackScreenProps<ConcertNavParamList, "VenueDetail">;

export function VenueDetailScreen({ route }: Props) {
  const { venueId } = route.params;
  const { venue, isLoading, isError } = useVenue(venueId);

  if (isLoading) {
    return (
      <View className="flex-1 bg-background">
        <Skeleton className="w-full h-[240px] rounded-none" />
        <View className="p-4 gap-4">
          <Skeleton className="w-[70%] h-6" />
          <Skeleton className="w-full h-24" />
        </View>
      </View>
    );
  }

  if (isError || !venue) {
    return (
      <View className="flex-1 bg-background">
        <ErrorState message="Failed to load venue." />
      </View>
    );
  }

  return (
    <Screen scroll padded={false}>
      <EditableProvider editMode={false}>
        <VenueDetails venue={venue} />
      </EditableProvider>
    </Screen>
  );
}
