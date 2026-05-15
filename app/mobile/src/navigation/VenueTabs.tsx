import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { PlaceholderScreen } from "../screens/shared/PlaceholderScreen";
import type { VenueTabParamList } from "./types";

const Tab = createBottomTabNavigator<VenueTabParamList>();

export function VenueTabs() {
  return (
    <Tab.Navigator>
      <Tab.Screen name="Home" children={() => <PlaceholderScreen name="Home" />} />
      <Tab.Screen name="Concerts" children={() => <PlaceholderScreen name="Concerts" />} />
      <Tab.Screen name="Messages" children={() => <PlaceholderScreen name="Messages" />} />
      <Tab.Screen name="Profile" children={() => <PlaceholderScreen name="Profile" />} />
    </Tab.Navigator>
  );
}
