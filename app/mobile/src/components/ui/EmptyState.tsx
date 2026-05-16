import { View, type ViewProps } from "react-native";
import type { LucideIcon } from "lucide-react-native";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { theme } from "../../lib/theme";

interface Props extends ViewProps {
  icon?: LucideIcon;
  title: string;
  description?: string;
  action?: { label: string; onPress: () => void };
}

export function EmptyState({ icon: Icon, title, description, action, className, ...props }: Readonly<Props>) {
  return (
    <View className={cn("flex-1 items-center justify-center gap-3 px-8 py-16", className)} {...props}>
      {Icon && <Icon size={48} color={theme.mutedForeground} strokeWidth={1.5} />}
      <View className="items-center gap-1">
        <Text className="text-base font-semibold text-foreground text-center">{title}</Text>
        {description && (
          <Text className="text-sm text-muted-foreground text-center">{description}</Text>
        )}
      </View>
      {action && (
        <Button variant="outline" onPress={action.onPress} size="sm">
          <Text>{action.label}</Text>
        </Button>
      )}
    </View>
  );
}
