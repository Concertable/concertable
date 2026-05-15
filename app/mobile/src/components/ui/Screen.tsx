import { SafeAreaView, ScrollView, View, type ViewProps } from "react-native";

interface Props extends ViewProps {
  children: React.ReactNode;
  scroll?: boolean;
  padded?: boolean;
  header?: React.ReactNode;
}

export function Screen({ children, scroll, padded = true, header, className, ...props }: Readonly<Props>) {
  if (scroll) {
    return (
      <SafeAreaView className={`flex-1 bg-background ${className ?? ""}`} {...props}>
        {header}
        <ScrollView
          contentContainerClassName={`flex-grow ${padded ? "p-4" : ""}`}
          showsVerticalScrollIndicator={false}
        >
          {children}
        </ScrollView>
      </SafeAreaView>
    );
  }
  return (
    <SafeAreaView className={`flex-1 bg-background ${className ?? ""}`} {...props}>
      {header}
      <View className={`flex-1 ${padded ? "p-4" : ""}`}>
        {children}
      </View>
    </SafeAreaView>
  );
}
