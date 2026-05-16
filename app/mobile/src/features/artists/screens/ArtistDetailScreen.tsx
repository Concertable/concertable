import { View } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { useArtist } from "@concertable/shared/features/artists";
import { EditableProvider } from "@concertable/shared/providers";
import { Screen } from "@/components/ui/Screen";
import { Skeleton } from "@/components/ui/skeleton";
import { ErrorState } from "@/components/ui/ErrorState";
import { ArtistDetails } from "../components/ArtistDetails";
import type { ConcertNavParamList } from "../../../navigation/types";

type Props = NativeStackScreenProps<ConcertNavParamList, "ArtistDetail">;

export function ArtistDetailScreen({ route }: Props) {
  const { artistId } = route.params;
  const { artist, isLoading, isError } = useArtist(artistId);

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

  if (isError || !artist) {
    return (
      <View className="flex-1 bg-background">
        <ErrorState message="Failed to load artist." />
      </View>
    );
  }

  return (
    <Screen scroll padded={false}>
      <EditableProvider editMode={false}>
        <ArtistDetails artist={artist} />
      </EditableProvider>
    </Screen>
  );
}
