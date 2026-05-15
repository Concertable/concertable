import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { TicketsScreen } from "../features/concerts/screens/TicketsScreen";
import { TicketDetailScreen } from "../features/concerts/screens/TicketDetailScreen";
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
