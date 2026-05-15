import { Pressable, Text, ActivityIndicator, type PressableProps } from "react-native";

interface Props extends PressableProps {
  children: React.ReactNode;
  variant?: "default" | "outline" | "ghost" | "destructive";
  size?: "sm" | "md" | "lg";
  loading?: boolean;
}

const variantClasses: Record<NonNullable<Props["variant"]>, string> = {
  default: "bg-primary",
  outline: "border border-border bg-transparent",
  ghost: "bg-transparent",
  destructive: "bg-destructive",
};

const textClasses: Record<NonNullable<Props["variant"]>, string> = {
  default: "text-primary-foreground",
  outline: "text-foreground",
  ghost: "text-foreground",
  destructive: "text-destructive-foreground",
};

const sizeClasses: Record<NonNullable<Props["size"]>, string> = {
  sm: "px-3 py-1.5",
  md: "px-4 py-2.5",
  lg: "px-6 py-3.5",
};

const textSizeClasses: Record<NonNullable<Props["size"]>, string> = {
  sm: "text-sm",
  md: "text-base",
  lg: "text-lg",
};

export function Button({
  children,
  variant = "default",
  size = "md",
  loading,
  disabled,
  className,
  ...props
}: Readonly<Props>) {
  const isDisabled = disabled || loading;
  return (
    <Pressable
      className={`flex-row items-center justify-center rounded-2xl gap-2 ${variantClasses[variant]} ${sizeClasses[size]} ${isDisabled ? "opacity-50" : ""} ${className ?? ""}`}
      disabled={isDisabled}
      {...props}
    >
      {loading && (
        <ActivityIndicator
          size="small"
          color={variant === "default" || variant === "destructive" ? "white" : undefined}
        />
      )}
      <Text className={`font-semibold ${textClasses[variant]} ${textSizeClasses[size]}`}>
        {children}
      </Text>
    </Pressable>
  );
}
