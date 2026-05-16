import { View } from "react-native";
import { useRoute } from "@react-navigation/native";
import type { RouteProp } from "@react-navigation/native";
import { useConcert } from "@concertable/shared/features/concerts";
import { ConcertDetails } from "../components/ConcertDetails";
import { ErrorState } from "@/components/ui/ErrorState";
import { Skeleton } from "@/components/ui/skeleton";
import type { ConcertNavParamList } from "../../../navigation/types";

type ConcertDetailRoute = RouteProp<ConcertNavParamList, "ConcertDetail">;

export function ConcertDetailScreen() {
  const route = useRoute<ConcertDetailRoute>();
  const { concert, isLoading, isError } = useConcert(route.params.concertId);

  if (isLoading) {
    return (
      <View className="flex-1 bg-background">
        <Skeleton className="w-full h-[280px] rounded-none" />
        <View className="p-4 gap-4">
          <Skeleton className="w-[70%] h-7" />
          <Skeleton className="w-1/2 h-4" />
          <Skeleton className="w-full h-12 rounded-xl" />
          <Skeleton className="w-full h-[120px]" />
        </View>
      </View>
    );
  }

  if (isError || !concert) {
    return (
      <View className="flex-1 bg-background">
        <ErrorState message="Failed to load concert." />
      </View>
    );
  }

  return <ConcertDetails concert={concert} />;
}
