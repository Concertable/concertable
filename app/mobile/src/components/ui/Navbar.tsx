import { View } from "react-native";
import { Logo } from "./Logo";

export function Navbar() {
  return (
    <View className="flex-row items-center px-4 py-3 bg-primary">
      <Logo size="sm" withWordmark />
    </View>
  );
}
