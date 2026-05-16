import { NavigationContainer } from "@react-navigation/native";
import { ActivityIndicator, View } from "react-native";
import { useAuthStore, isVenueManager, isArtistManager } from "@concertable/shared/features/auth";
import { useAuthInit } from "shared/auth/useAuthInit";
import { VenueTabs } from "shared/navigation/VenueTabs";
import { ArtistTabs } from "shared/navigation/ArtistTabs";
import { RoleSelectScreen } from "../screens/RoleSelectScreen";
import { WrongAppScreen } from "../screens/WrongAppScreen";
import { theme } from "shared/lib/theme";

export function RootNavigator() {
  const user = useAuthStore((s) => s.user);
  const isReady = useAuthInit();

  if (!isReady) {
    return (
      <View style={{ flex: 1, alignItems: "center", justifyContent: "center" }}>
        <ActivityIndicator size="large" color={theme.primary} />
      </View>
    );
  }

  if (!user) return <RoleSelectScreen />;
  if (isVenueManager(user)) return <NavigationContainer><VenueTabs /></NavigationContainer>;
  if (isArtistManager(user)) return <NavigationContainer><ArtistTabs /></NavigationContainer>;
  return <WrongAppScreen />;
}
