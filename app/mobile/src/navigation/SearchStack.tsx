import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { SearchScreen } from "../screens/customer/SearchScreen";
import { ConcertDetailScreen } from "../screens/customer/ConcertDetailScreen";
import { TicketCheckoutScreen } from "../screens/customer/TicketCheckoutScreen";
import { CheckoutSuccessScreen } from "../screens/customer/CheckoutSuccessScreen";
import type { SearchStackParamList } from "./types";

const Stack = createNativeStackNavigator<SearchStackParamList>();

export function SearchStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="SearchMain" component={SearchScreen} options={{ title: "Search" }} />
      <Stack.Screen name="ConcertDetail" component={ConcertDetailScreen} options={{ title: "Concert" }} />
      <Stack.Screen name="TicketCheckout" component={TicketCheckoutScreen} options={{ title: "Buy Ticket" }} />
      <Stack.Screen name="CheckoutSuccess" component={CheckoutSuccessScreen} options={{ headerShown: false }} />
    </Stack.Navigator>
  );
}
