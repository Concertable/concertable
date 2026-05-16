import { View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Logo } from "./Logo";

export function Navbar() {
  const { top } = useSafeAreaInsets();
  return (
    <View className="bg-primary" style={{ paddingTop: top }}>
      <View className="flex-row items-center px-4 py-3">
        <Logo size="sm" withWordmark />
      </View>
    </View>
  );
}
