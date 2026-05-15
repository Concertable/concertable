import { useState } from "react";
import { ScrollView, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useNavigation } from "@react-navigation/native";
import { useMyPreferenceQuery, useUpdateMyPreferenceMutation, useCreateMyPreferenceMutation } from "@concertable/shared/features/customer";
import { useGenresQuery } from "@concertable/shared/features/search";
import { GenreChips } from "../../../components/ui/GenreChips";
import { Button } from "../../../components/ui/Button";
import { Skeleton } from "../../../components/ui/Skeleton";
import { notify } from "../../../lib/toast";

const RADIUS_PRESETS = [5, 10, 25, 50, 100] as const;

export function PreferencesScreen() {
  const nav = useNavigation();
  const { data: preference, isLoading } = useMyPreferenceQuery();
  const { data: allGenres } = useGenresQuery();
  const updatePreference = useUpdateMyPreferenceMutation();
  const createPreference = useCreateMyPreferenceMutation();

  const [genreIds, setGenreIds] = useState<number[]>(() =>
    preference?.genres.map((g) => g.id) ?? [],
  );
  const [radiusKm, setRadiusKm] = useState<number>(() => preference?.radiusKm ?? 25);

  async function handleSave() {
    try {
      if (preference?.id) {
        await updatePreference.mutateAsync({
          id: preference.id,
          data: {
            id: preference.id,
            user: preference.user,
            radiusKm,
            genres: (allGenres ?? []).filter((g) => genreIds.includes(g.id)),
          },
        });
      } else {
        await createPreference.mutateAsync({ radiusKm, genres: [] });
      }
      notify("Preferences saved", "success");
      nav.goBack();
    } catch {
      notify("Failed to save preferences", "error");
    }
  }

  const saving = updatePreference.isPending || createPreference.isPending;

  if (isLoading) {
    return (
      <View className="flex-1 bg-background p-4 gap-4">
        <Skeleton width="100%" height={48} className="rounded-xl" />
        <Skeleton width="100%" height={120} className="rounded-xl" />
      </View>
    );
  }

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <ScrollView contentContainerStyle={{ padding: 16, gap: 20 }} showsVerticalScrollIndicator={false}>
        <View className="gap-3">
          <Text className="text-base font-semibold text-foreground">Search Radius</Text>
          <View className="flex-row flex-wrap gap-2">
            {RADIUS_PRESETS.map((r) => (
              <Button
                key={r}
                variant={radiusKm === r ? "default" : "outline"}
                size="sm"
                onPress={() => setRadiusKm(r)}
              >
                {`${r}km`}
              </Button>
            ))}
          </View>
        </View>

        <View className="gap-3">
          <Text className="text-base font-semibold text-foreground">Preferred Genres</Text>
          {allGenres && (
            <GenreChips
              genres={allGenres}
              selected={genreIds}
              onToggle={(id) =>
                setGenreIds((prev) => (prev.includes(id) ? prev.filter((g) => g !== id) : [...prev, id]))
              }
            />
          )}
        </View>
      </ScrollView>

      <View className="px-4 pt-3 pb-6 border-t border-border">
        <Button loading={saving} onPress={handleSave} size="lg">Save</Button>
      </View>
    </SafeAreaView>
  );
}
