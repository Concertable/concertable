import { Linking, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Logo } from "@/components/ui/Logo";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { useAuthStore } from "@concertable/shared/features/auth";
import { useLogout } from "shared/auth/useLogout";

export function WrongAppScreen() {
  const { top, bottom } = useSafeAreaInsets();
  const { logout } = useLogout();
  const user = useAuthStore((s) => s.user);
  const isCustomer = user?.role === "Customer";

  return (
    <View className="flex-1 bg-background">
      <View
        className="bg-primary items-center px-6 pb-8 rounded-b-3xl"
        style={{ paddingTop: top + 32 }}
      >
        <Logo size="md" withWordmark style={{ tintColor: "white" }} />
      </View>

      <View
        className="flex-1 items-center justify-center px-6"
        style={{ paddingBottom: bottom + 24 }}
      >
        <Text className="text-xl font-bold text-foreground text-center">
          {isCustomer ? "Wrong app" : "Setup incomplete"}
        </Text>
        <Text className="text-sm text-muted-foreground text-center mt-2 px-4">
          {isCustomer
            ? "This account is registered as a customer. Download the Concertable app to buy tickets and discover concerts."
            : "Your account isn't set up as a venue or artist yet. Complete registration at concertable.com to get started."}
        </Text>
        <View className="mt-8 w-full gap-3">
          {!isCustomer && (
            <Button onPress={() => Linking.openURL("https://business.concertable.com")}>
              <Text>Complete registration</Text>
            </Button>
          )}
          <Button variant="outline" onPress={logout}>
            <Text>Sign out</Text>
          </Button>
        </View>
      </View>
    </View>
  );
}
