import { View, Text } from "react-native";

interface BadgeProps {
  children: React.ReactNode;
  variant?: "default" | "secondary" | "destructive" | "outline";
  className?: string;
}

const variantClasses: Record<NonNullable<BadgeProps["variant"]>, { container: string; text: string }> = {
  default: { container: "bg-black", text: "text-white" },
  secondary: { container: "bg-gray-100", text: "text-gray-800" },
  destructive: { container: "bg-red-100", text: "text-red-700" },
  outline: { container: "border border-gray-300", text: "text-gray-800" },
};

export function Badge({ children, variant = "secondary", className }: BadgeProps) {
  const { container, text } = variantClasses[variant];
  return (
    <View className={`rounded-full px-2.5 py-0.5 self-start ${container} ${className ?? ""}`}>
      <Text className={`text-xs font-medium ${text}`}>{children}</Text>
    </View>
  );
}
