import { View } from "react-native";
import { useRoute } from "@react-navigation/native";
import type { RouteProp } from "@react-navigation/native";
import { useConcert } from "@concertable/shared/features/concerts";
import { ConcertDetails } from "../components/ConcertDetails";
import { ErrorState } from "../../../components/ui/ErrorState";
import { Skeleton } from "../../../components/ui/Skeleton";
import type { ConcertNavParamList } from "../../../navigation/types";

type ConcertDetailRoute = RouteProp<ConcertNavParamList, "ConcertDetail">;

export function ConcertDetailScreen() {
  const route = useRoute<ConcertDetailRoute>();
  const { concert, isLoading, isError } = useConcert(route.params.concertId);

  if (isLoading) {
    return (
      <View className="flex-1 bg-background">
        <Skeleton width="100%" height={280} className="rounded-none" />
        <View className="p-4 gap-4">
          <Skeleton width="70%" height={28} />
          <Skeleton width="50%" height={16} />
          <Skeleton width="100%" height={48} className="rounded-xl" />
          <Skeleton width="100%" height={120} />
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
