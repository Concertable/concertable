import { View } from "react-native";
import { MessageCircle } from "lucide-react-native";
import { Screen } from "@/components/ui/Screen";
import { Navbar } from "@/components/ui/Navbar";
import { EmptyState } from "@/components/ui/EmptyState";

export function MessagesScreen() {
  return (
    <Screen header={<Navbar />}>
      <View className="flex-1 justify-center">
        <EmptyState
          icon={MessageCircle}
          title="Messaging coming soon"
          description="We're working on it. Check back soon."
        />
      </View>
    </Screen>
  );
}
