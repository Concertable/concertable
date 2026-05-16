import { NavigationContainer } from "@react-navigation/native";
import { ActivityIndicator, View } from "react-native";
import { useAuthInit } from "shared/auth/useAuthInit";
import { ArtistTabs } from "shared/navigation/ArtistTabs";

export function RootNavigator() {
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
      <ArtistTabs />
    </NavigationContainer>
  );
}
