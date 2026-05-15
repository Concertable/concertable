import { View, Text, Image, type ViewProps } from "react-native";

interface Props extends ViewProps {
  uri?: string | null;
  title: string;
  subtitle?: string;
  height?: number;
  trailing?: React.ReactNode;
}

export function Hero({ uri, title, subtitle, height = 280, trailing, className, ...props }: Readonly<Props>) {
  return (
    <View style={{ height }} className={`relative overflow-hidden ${className ?? ""}`} {...props}>
      {uri ? (
        <Image
          source={{ uri }}
          style={{ position: "absolute", inset: 0, width: "100%", height: "100%" }}
          resizeMode="cover"
        />
      ) : (
        <View style={{ position: "absolute", inset: 0 }} className="bg-muted" />
      )}
      <View style={{ position: "absolute", inset: 0 }} className="bg-foreground/40" />
      <View className="absolute bottom-0 left-0 right-0 p-4 flex-row items-end justify-between">
        <View className="flex-1 mr-3 gap-1">
          <Text className="text-2xl font-bold text-white" numberOfLines={2}>{title}</Text>
          {subtitle && <Text className="text-sm text-white/80">{subtitle}</Text>}
        </View>
        {trailing}
      </View>
    </View>
  );
}
