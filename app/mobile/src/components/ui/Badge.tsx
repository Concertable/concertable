import { View, Text } from "react-native";

interface Props {
  children: React.ReactNode;
  variant?: "default" | "secondary" | "destructive" | "outline" | "success" | "warning";
  className?: string;
}

const variantClasses: Record<NonNullable<Props["variant"]>, { container: string; text: string }> = {
  default: { container: "bg-primary", text: "text-primary-foreground" },
  secondary: { container: "bg-muted", text: "text-muted-foreground" },
  destructive: { container: "bg-destructive/15", text: "text-destructive" },
  outline: { container: "border border-border", text: "text-foreground" },
  success: { container: "bg-success/15", text: "text-success" },
  warning: { container: "bg-warning/15", text: "text-warning" },
};

export function Badge({ children, variant = "secondary", className }: Readonly<Props>) {
  const { container, text } = variantClasses[variant];
  return (
    <View className={`rounded-full px-2.5 py-0.5 self-start ${container} ${className ?? ""}`}>
      <Text className={`text-xs font-medium ${text}`}>{children}</Text>
    </View>
  );
}
