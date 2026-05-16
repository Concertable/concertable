import { NavigationContainer } from "@react-navigation/native";
import { ActivityIndicator, View } from "react-native";
import { useAuthStore, type Role } from "@concertable/shared/features/auth";
import { useAuthInit } from "../auth/useAuthInit";
import { CustomerTabs } from "./CustomerTabs";
import { ArtistTabs } from "./ArtistTabs";
import { VenueTabs } from "./VenueTabs";

function chooseTabs(role?: Role) {
  switch (role) {
    case "VenueManager":
      return <VenueTabs />;
    case "ArtistManager":
      return <ArtistTabs />;
    default:
      return <CustomerTabs />;
  }
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

  return <NavigationContainer>{chooseTabs(user?.role)}</NavigationContainer>;
}
