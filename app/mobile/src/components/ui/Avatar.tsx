import { Image, View, Text } from "react-native";

interface AvatarProps {
  uri?: string | null;
  name?: string;
  size?: number;
  className?: string;
}

export function Avatar({ uri, name, size = 40, className }: AvatarProps) {
  const initials = name
    ? name.split(" ").map((w) => w[0]).slice(0, 2).join("").toUpperCase()
    : "?";

  return uri ? (
    <Image
      source={{ uri }}
      style={{ width: size, height: size, borderRadius: size / 2 }}
      className={className}
    />
  ) : (
    <View
      style={{ width: size, height: size, borderRadius: size / 2 }}
      className={`bg-gray-200 items-center justify-center ${className ?? ""}`}
    >
      <Text className="text-gray-600 font-semibold" style={{ fontSize: size * 0.38 }}>
        {initials}
      </Text>
    </View>
  );
}
