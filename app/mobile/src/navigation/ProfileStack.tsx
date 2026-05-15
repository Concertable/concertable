import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { ProfileScreen } from "../features/user/screens/ProfileScreen";
import { EditProfileScreen } from "../features/user/screens/EditProfileScreen";
import { LocationScreen } from "../features/user/screens/LocationScreen";
import { PreferencesScreen } from "../features/user/screens/PreferencesScreen";
import type { ProfileStackParamList } from "./types";

const Stack = createNativeStackNavigator<ProfileStackParamList>();

export function ProfileStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="ProfileMain" component={ProfileScreen} options={{ title: "Profile" }} />
      <Stack.Screen name="EditProfile" component={EditProfileScreen} options={{ title: "Edit Profile" }} />
      <Stack.Screen name="Location" component={LocationScreen} options={{ title: "Location" }} />
      <Stack.Screen name="Preferences" component={PreferencesScreen} options={{ title: "Preferences" }} />
    </Stack.Navigator>
  );
}
