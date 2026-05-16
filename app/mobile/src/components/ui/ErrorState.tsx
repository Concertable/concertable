import { View, type ViewProps } from "react-native";
import { AlertCircle } from "lucide-react-native";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { theme } from "../../lib/theme";

interface Props extends ViewProps {
  message?: string;
  onRetry?: () => void;
}

export function ErrorState({ message = "Something went wrong.", onRetry, className, ...props }: Readonly<Props>) {
  return (
    <View className={cn("flex-1 items-center justify-center gap-3 px-8 py-16", className)} {...props}>
      <AlertCircle size={48} color={theme.destructive} strokeWidth={1.5} />
      <Text className="text-sm text-muted-foreground text-center">{message}</Text>
      {onRetry && (
        <Button variant="outline" onPress={onRetry} size="sm">
          <Text>Try again</Text>
        </Button>
      )}
    </View>
  );
}
