import { View, Text } from "react-native";
import { Star } from "lucide-react-native";
import { theme } from "../../lib/theme";

interface Props {
  rating?: number | null;
  size?: number;
  className?: string;
}

export function RatingStars({ rating, size = 14, className }: Readonly<Props>) {
  if (rating == null) return null;
  return (
    <View className={`flex-row items-center gap-1 ${className ?? ""}`}>
      <Star size={size} color={theme.gold} fill={theme.gold} />
      <Text className="text-xs font-medium text-gold">{rating.toFixed(1)}</Text>
    </View>
  );
}
