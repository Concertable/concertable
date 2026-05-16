import { ProfileScreen } from "../features/user/screens/ProfileScreen";
import { EditProfileScreen } from "../features/user/screens/EditProfileScreen";
import { LocationScreen } from "../features/user/screens/LocationScreen";
import { PreferencesScreen } from "../features/user/screens/PreferencesScreen";
import { createAppStack } from "./createAppStack";
import type { ProfileStackParamList } from "./types";

const Stack = createAppStack<ProfileStackParamList>();

export function ProfileStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="ProfileMain" component={ProfileScreen} options={{ headerShown: false }} />
      <Stack.Screen name="EditProfile" component={EditProfileScreen} options={{ title: "Edit Profile" }} />
      <Stack.Screen name="Location" component={LocationScreen} options={{ title: "Location" }} />
      <Stack.Screen name="Preferences" component={PreferencesScreen} options={{ title: "Preferences" }} />
    </Stack.Navigator>
  );
}
