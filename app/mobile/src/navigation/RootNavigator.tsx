import { NavigationContainer } from "@react-navigation/native";
import { ActivityIndicator, View } from "react-native";
import { useAuthStore, isVenueManager, isArtistManager } from "@concertable/shared/features/auth";
import { useAuthInit } from "../auth/useAuthInit";
import { AuthStack } from "./AuthStack";
import { CustomerTabs } from "./CustomerTabs";
import { ArtistTabs } from "./ArtistTabs";
import { VenueTabs } from "./VenueTabs";
import type { User } from "@concertable/shared/features/auth";

function AppNavigator({ user }: { user: User }) {
  if (isVenueManager(user)) return <VenueTabs />;
  if (isArtistManager(user)) return <ArtistTabs />;
  return <CustomerTabs />;
}

export function RootNavigator() {
  const user = useAuthStore((s) => s.user);
  const isReady = useAuthInit();

  if (!isReady) {
    return (
      <View style={{ flex: 1, alignItems: "center", justifyContent: "center" }}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return (
    <NavigationContainer>
      {user ? <AppNavigator user={user} /> : <AuthStack />}
    </NavigationContainer>
  );
}
