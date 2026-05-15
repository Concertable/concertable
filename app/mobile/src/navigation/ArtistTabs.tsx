import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { PlaceholderScreen } from "../screens/shared/PlaceholderScreen";
import type { ArtistTabParamList } from "./types";

const Tab = createBottomTabNavigator<ArtistTabParamList>();

export function ArtistTabs() {
  return (
    <Tab.Navigator>
      <Tab.Screen name="Home" children={() => <PlaceholderScreen name="Home" />} />
      <Tab.Screen name="Search" children={() => <PlaceholderScreen name="Search" />} />
      <Tab.Screen name="Applications" children={() => <PlaceholderScreen name="Applications" />} />
      <Tab.Screen name="Messages" children={() => <PlaceholderScreen name="Messages" />} />
      <Tab.Screen name="Profile" children={() => <PlaceholderScreen name="Profile" />} />
    </Tab.Navigator>
  );
}
