import { useEffect, useState } from "react";
import { View, Text, ActivityIndicator } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { useStripe } from "@stripe/stripe-react-native";
import { useQueryClient } from "@tanstack/react-query";
import { useTicketCheckoutQuery } from "@concertable/shared/features/concerts";
import { Screen } from "../../components/ui/Screen";
import { Button } from "../../components/ui/Button";
import { notify } from "../../lib/toast";
import type { ConcertNavParamList } from "../../navigation/types";

type Props = NativeStackScreenProps<ConcertNavParamList, "TicketCheckout">;

export function TicketCheckoutScreen({ navigation, route }: Props) {
  const { concertId } = route.params;
  const { initPaymentSheet, presentPaymentSheet } = useStripe();
  const queryClient = useQueryClient();
  const [ready, setReady] = useState(false);
  const [paying, setPaying] = useState(false);

  const { data: checkout, isLoading, isError } = useTicketCheckoutQuery(concertId);

  useEffect(() => {
    if (!checkout) return;

    initPaymentSheet({
      merchantDisplayName: "Concertable",
      paymentIntentClientSecret: checkout.session.clientSecret,
      customerEphemeralKeySecret: checkout.session.customerSession ?? undefined,
      customerId: checkout.session.customerId ?? undefined,
      allowsDelayedPaymentMethods: false,
    }).then(({ error }) => {
      if (error) notify(error.message, "error");
      else setReady(true);
    });
  }, [checkout]);

  async function handlePay() {
    setPaying(true);
    const { error } = await presentPaymentSheet();
    setPaying(false);

    if (error) {
      if (error.code !== "Canceled") notify(error.message, "error");
      return;
    }

    await queryClient.invalidateQueries({ queryKey: ["tickets", "upcoming"] });
    navigation.replace("CheckoutSuccess");
  }

  if (isLoading)
    return (
      <Screen>
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" />
          <Text className="text-gray-500 mt-3">Preparing checkout…</Text>
        </View>
      </Screen>
    );

  if (isError || !checkout)
    return (
      <Screen>
        <View className="flex-1 items-center justify-center">
          <Text className="text-red-500">Failed to start checkout.</Text>
        </View>
      </Screen>
    );

  return (
    <Screen>
      <View className="flex-1 justify-between">
        <View className="gap-4">
          <Text className="text-2xl font-bold text-gray-900">Order Summary</Text>
          <View className="bg-gray-50 rounded-2xl p-4 gap-2">
            <View className="flex-row justify-between">
              <Text className="text-gray-600">1 × Ticket</Text>
              <Text className="font-semibold text-gray-900">£{checkout.price.toFixed(2)}</Text>
            </View>
            <View className="h-px bg-gray-200 my-1" />
            <View className="flex-row justify-between">
              <Text className="font-semibold text-gray-900">Total</Text>
              <Text className="font-bold text-gray-900 text-lg">£{checkout.price.toFixed(2)}</Text>
            </View>
          </View>
        </View>

        <Button loading={paying} disabled={!ready} onPress={handlePay} size="lg">
          {ready ? `Pay £${checkout.price.toFixed(2)}` : "Loading…"}
        </Button>
      </View>
    </Screen>
  );
}
