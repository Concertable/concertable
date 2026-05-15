import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { HomeStack } from "./HomeStack";
import { SearchStack } from "./SearchStack";
import { TicketsStack } from "./TicketsStack";
import { ProfileScreen } from "../screens/customer/ProfileScreen";
import { PlaceholderScreen } from "../screens/shared/PlaceholderScreen";
import type { CustomerTabParamList } from "./types";

const Tab = createBottomTabNavigator<CustomerTabParamList>();

export function CustomerTabs() {
  return (
    <Tab.Navigator screenOptions={{ headerShown: false }}>
      <Tab.Screen name="HomeTab" component={HomeStack} options={{ title: "Home" }} />
      <Tab.Screen name="SearchTab" component={SearchStack} options={{ title: "Search" }} />
      <Tab.Screen name="TicketsTab" component={TicketsStack} options={{ title: "Tickets" }} />
      <Tab.Screen name="Messages" children={() => <PlaceholderScreen name="Messages" />} options={{ title: "Messages" }} />
      <Tab.Screen name="Profile" component={ProfileScreen} />
    </Tab.Navigator>
  );
}
