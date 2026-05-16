import { cn } from "@/lib/utils";
import { ScrollView, View, type ViewProps } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

interface Props extends ViewProps {
  children: React.ReactNode;
  scroll?: boolean;
  padded?: boolean;
  header?: React.ReactNode;
}

export function Screen({ children, scroll, padded = true, header, className, ...props }: Readonly<Props>) {
  const { bottom } = useSafeAreaInsets();

  if (scroll) {
    return (
      <View className={cn("flex-1 bg-background", className)} style={{ paddingBottom: bottom }} {...props}>
        {header}
        <ScrollView
          contentContainerClassName={cn("flex-grow", padded && "p-4")}
          showsVerticalScrollIndicator={false}
        >
          {children}
        </ScrollView>
      </View>
    );
  }
  return (
    <View className={cn("flex-1 bg-background", className)} style={{ paddingBottom: bottom }} {...props}>
      {header}
      <View style={{ flex: 1, flexDirection: "column", ...(padded && { padding: 16 }) }}>
        {children}
      </View>
    </View>
  );
}
