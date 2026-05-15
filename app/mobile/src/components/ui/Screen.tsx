import { SafeAreaView, ScrollView, type ViewProps } from "react-native";

interface ScreenProps extends ViewProps {
  children: React.ReactNode;
  scroll?: boolean;
}

export function Screen({ children, scroll, className, ...props }: ScreenProps) {
  const inner = scroll ? (
    <ScrollView contentContainerClassName="flex-grow p-4">{children}</ScrollView>
  ) : (
    children
  );
  return (
    <SafeAreaView className={`flex-1 bg-white ${className ?? ""}`} {...props}>
      {inner}
    </SafeAreaView>
  );
}
