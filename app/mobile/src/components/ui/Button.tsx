import { Pressable, Text, ActivityIndicator, type PressableProps } from "react-native";

interface ButtonProps extends PressableProps {
  children: React.ReactNode;
  variant?: "default" | "outline" | "ghost" | "destructive";
  size?: "sm" | "md" | "lg";
  loading?: boolean;
}

const variantClasses: Record<NonNullable<ButtonProps["variant"]>, string> = {
  default: "bg-black",
  outline: "border border-gray-300 bg-transparent",
  ghost: "bg-transparent",
  destructive: "bg-red-600",
};

const textClasses: Record<NonNullable<ButtonProps["variant"]>, string> = {
  default: "text-white",
  outline: "text-black",
  ghost: "text-black",
  destructive: "text-white",
};

const sizeClasses: Record<NonNullable<ButtonProps["size"]>, string> = {
  sm: "px-3 py-1.5",
  md: "px-4 py-2.5",
  lg: "px-6 py-3.5",
};

const textSizeClasses: Record<NonNullable<ButtonProps["size"]>, string> = {
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
}: ButtonProps) {
  const isDisabled = disabled || loading;
  return (
    <Pressable
      className={`flex-row items-center justify-center rounded-xl gap-2 ${variantClasses[variant]} ${sizeClasses[size]} ${isDisabled ? "opacity-50" : ""} ${className ?? ""}`}
      disabled={isDisabled}
      {...props}
    >
      {loading && <ActivityIndicator size="small" color={variant === "default" || variant === "destructive" ? "#fff" : "#000"} />}
      <Text className={`font-semibold ${textClasses[variant]} ${textSizeClasses[size]}`}>
        {children}
      </Text>
    </Pressable>
  );
}
