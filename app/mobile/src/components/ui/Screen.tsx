import { cn } from "@/lib/utils";
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
      <SafeAreaView className={cn("flex-1 bg-background", className)} {...props}>
        {header}
        <ScrollView
          contentContainerClassName={cn("flex-grow", padded && "p-4")}
          showsVerticalScrollIndicator={false}
        >
          {children}
        </ScrollView>
      </SafeAreaView>
    );
  }
  return (
    <SafeAreaView className={cn("flex-1 bg-background", className)} {...props}>
      {header}
      <View style={{ flex: 1, flexDirection: "column", ...(padded && { padding: 16 }) }}>
        {children}
      </View>
    </SafeAreaView>
  );
}
