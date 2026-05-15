import { TextInput, View, Text, type TextInputProps } from "react-native";

interface InputProps extends TextInputProps {
  label?: string;
  error?: string;
}

export function Input({ label, error, className, ...props }: InputProps) {
  return (
    <View className="gap-1">
      {label && <Text className="text-sm font-medium text-gray-700">{label}</Text>}
      <TextInput
        className={`border rounded-xl px-3 py-2.5 text-base text-black ${error ? "border-red-500" : "border-gray-300"} ${className ?? ""}`}
        placeholderTextColor="#9ca3af"
        {...props}
      />
      {error && <Text className="text-sm text-red-500">{error}</Text>}
    </View>
  );
}
