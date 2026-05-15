import { View, Text, Pressable, type ViewProps } from "react-native";

interface Props<T extends string> extends ViewProps {
  options: { value: T; label: string }[];
  value: T;
  onChange: (value: T) => void;
}

export function SegmentedControl<T extends string>({
  options,
  value,
  onChange,
  className,
  ...props
}: Readonly<Props<T>>) {
  return (
    <View className={`flex-row bg-muted rounded-xl p-1 gap-1 ${className ?? ""}`} {...props}>
      {options.map((opt) => {
        const active = opt.value === value;
        return (
          <Pressable
            key={opt.value}
            onPress={() => onChange(opt.value)}
            className={`flex-1 py-2 items-center rounded-lg ${active ? "bg-primary" : ""}`}
          >
            <Text
              className={`text-sm font-semibold ${active ? "text-primary-foreground" : "text-muted-foreground"}`}
            >
              {opt.label}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
