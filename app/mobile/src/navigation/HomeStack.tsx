import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { HomeScreen } from "../features/search/screens/HomeScreen";
import { ConcertDetailScreen } from "../features/concerts/screens/ConcertDetailScreen";
import { TicketCheckoutScreen } from "../features/concerts/screens/TicketCheckoutScreen";
import { CheckoutSuccessScreen } from "../features/concerts/screens/CheckoutSuccessScreen";
import { ArtistDetailScreen } from "../features/artists/screens/ArtistDetailScreen";
import { VenueDetailScreen } from "../features/venues/screens/VenueDetailScreen";
import type { HomeStackParamList } from "./types";

const Stack = createNativeStackNavigator<HomeStackParamList>();

export function HomeStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="HomeMain" component={HomeScreen} options={{ headerShown: false }} />
      <Stack.Screen name="ConcertDetail" component={ConcertDetailScreen} options={{ title: "Concert" }} />
      <Stack.Screen name="TicketCheckout" component={TicketCheckoutScreen} options={{ title: "Buy Ticket" }} />
      <Stack.Screen name="CheckoutSuccess" component={CheckoutSuccessScreen} options={{ headerShown: false }} />
      <Stack.Screen name="ArtistDetail" component={ArtistDetailScreen} options={{ title: "Artist" }} />
      <Stack.Screen name="VenueDetail" component={VenueDetailScreen} options={{ title: "Venue" }} />
    </Stack.Navigator>
  );
}
