import { TextInput, View, Text, type TextInputProps } from "react-native";

interface Props extends TextInputProps {
  label?: string;
  error?: string;
}

export function Input({ label, error, className, ...props }: Readonly<Props>) {
  return (
    <View className="gap-1">
      {label && <Text className="text-sm font-medium text-foreground">{label}</Text>}
      <TextInput
        className={`border rounded-xl px-3 py-2.5 text-base bg-background text-foreground ${error ? "border-destructive" : "border-input"} ${className ?? ""}`}
        placeholderTextColor="hsl(240, 4%, 46%)"
        {...props}
      />
      {error && <Text className="text-sm text-destructive">{error}</Text>}
    </View>
  );
}
