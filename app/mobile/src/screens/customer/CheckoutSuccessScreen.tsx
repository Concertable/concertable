import { View, Text } from "react-native";
import { useNavigation } from "@react-navigation/native";
import type { BottomTabNavigationProp } from "@react-navigation/bottom-tabs";
import { Screen } from "../../components/ui/Screen";
import { Button } from "../../components/ui/Button";
import type { CustomerTabParamList } from "../../navigation/types";

export function CheckoutSuccessScreen() {
  const tabNav = useNavigation<BottomTabNavigationProp<CustomerTabParamList>>();

  return (
    <Screen>
      <View className="flex-1 items-center justify-center gap-6 px-6">
        <View className="w-20 h-20 rounded-full bg-green-100 items-center justify-center">
          <Text className="text-4xl">✓</Text>
        </View>
        <View className="items-center gap-2">
          <Text className="text-2xl font-bold text-gray-900">You're going!</Text>
          <Text className="text-gray-500 text-center">
            Your ticket has been confirmed. See you there!
          </Text>
        </View>
        <Button onPress={() => tabNav.navigate("TicketsTab", { screen: "TicketsMain" })} size="lg">
          View My Tickets
        </Button>
      </View>
    </Screen>
  );
}
