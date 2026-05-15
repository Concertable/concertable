import { View, type ViewProps } from "react-native";

export function Card({ children, className, ...props }: ViewProps) {
  return (
    <View
      className={`bg-white rounded-2xl border border-gray-200 p-4 ${className ?? ""}`}
      {...props}
    >
      {children}
    </View>
  );
}
