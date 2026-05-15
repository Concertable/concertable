import { View, Text, Pressable } from "react-native";
import type { Genre } from "@concertable/shared/types";

interface Props {
  genres: Genre[];
  selected?: number[];
  onToggle?: (id: number) => void;
  max?: number;
  className?: string;
}

export function GenreChips({ genres, selected, onToggle, max, className }: Readonly<Props>) {
  const visible = max ? genres.slice(0, max) : genres;
  const isSelectable = onToggle !== undefined;

  return (
    <View className={`flex-row flex-wrap gap-1.5 ${className ?? ""}`}>
      {visible.map((genre) => {
        const active = selected?.includes(genre.id) ?? false;
        if (isSelectable) {
          return (
            <Pressable
              key={genre.id}
              onPress={() => onToggle!(genre.id)}
              className={`rounded-full px-2.5 py-1 border ${active ? "bg-primary border-primary" : "border-border bg-background"}`}
            >
              <Text className={`text-xs font-medium ${active ? "text-primary-foreground" : "text-foreground"}`}>
                {genre.name}
              </Text>
            </Pressable>
          );
        }
        return (
          <View key={genre.id} className="rounded-full px-2.5 py-1 bg-muted">
            <Text className="text-xs font-medium text-muted-foreground">{genre.name}</Text>
          </View>
        );
      })}
    </View>
  );
}
