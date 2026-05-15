import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { HomeScreen } from "../screens/customer/HomeScreen";
import { ConcertDetailScreen } from "../screens/customer/ConcertDetailScreen";
import { TicketCheckoutScreen } from "../screens/customer/TicketCheckoutScreen";
import { CheckoutSuccessScreen } from "../screens/customer/CheckoutSuccessScreen";
import type { HomeStackParamList } from "./types";

const Stack = createNativeStackNavigator<HomeStackParamList>();

export function HomeStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="HomeMain" component={HomeScreen} options={{ title: "Home" }} />
      <Stack.Screen name="ConcertDetail" component={ConcertDetailScreen} options={{ title: "Concert" }} />
      <Stack.Screen name="TicketCheckout" component={TicketCheckoutScreen} options={{ title: "Buy Ticket" }} />
      <Stack.Screen name="CheckoutSuccess" component={CheckoutSuccessScreen} options={{ headerShown: false }} />
    </Stack.Navigator>
  );
}
