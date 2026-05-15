import { View, Text, Pressable, type ViewProps } from "react-native";
import { Minus, Plus } from "lucide-react-native";
import { theme } from "../../lib/theme";

interface Props extends ViewProps {
  value: number;
  onChange: (value: number) => void;
  min?: number;
  max?: number;
}

export function QuantitySelector({
  value,
  onChange,
  min = 1,
  max,
  className,
  ...props
}: Readonly<Props>) {
  const canDecrement = value > min;
  const canIncrement = max === undefined || value < max;

  return (
    <View className={`flex-row items-center gap-3 ${className ?? ""}`} {...props}>
      <Pressable
        onPress={() => canDecrement && onChange(value - 1)}
        disabled={!canDecrement}
        className={`w-9 h-9 rounded-full border border-border items-center justify-center ${!canDecrement ? "opacity-40" : ""}`}
      >
        <Minus size={16} color={theme.foreground} />
      </Pressable>
      <Text className="text-lg font-semibold text-foreground w-6 text-center">{value}</Text>
      <Pressable
        onPress={() => canIncrement && onChange(value + 1)}
        disabled={!canIncrement}
        className={`w-9 h-9 rounded-full border border-border items-center justify-center ${!canIncrement ? "opacity-40" : ""}`}
      >
        <Plus size={16} color={theme.foreground} />
      </Pressable>
    </View>
  );
}
