import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { TicketsScreen } from "../screens/customer/TicketsScreen";
import { TicketDetailScreen } from "../screens/customer/TicketDetailScreen";
import type { TicketsStackParamList } from "./types";

const Stack = createNativeStackNavigator<TicketsStackParamList>();

export function TicketsStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="TicketsMain" component={TicketsScreen} options={{ title: "My Tickets" }} />
      <Stack.Screen name="TicketDetail" component={TicketDetailScreen} options={{ title: "Ticket" }} />
    </Stack.Navigator>
  );
}
