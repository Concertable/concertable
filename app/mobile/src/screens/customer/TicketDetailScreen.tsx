import { View, Text, ScrollView } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { useUpcomingTicketsQuery, useTicketHistoryQuery } from "@concertable/shared/features/concerts";
import QRCode from "react-native-qrcode-svg";
import { Screen } from "../../components/ui/Screen";
import dayjs from "dayjs";
import type { TicketsStackParamList } from "../../navigation/types";

type Props = NativeStackScreenProps<TicketsStackParamList, "TicketDetail">;

export function TicketDetailScreen({ route }: Props) {
  const { ticketId } = route.params;
  const { data: upcoming } = useUpcomingTicketsQuery();
  const { data: history } = useTicketHistoryQuery();

  const ticket = [...(upcoming ?? []), ...(history ?? [])].find((t) => t.id === ticketId);

  if (!ticket)
    return (
      <Screen>
        <View className="flex-1 items-center justify-center">
          <Text className="text-gray-500">Ticket not found.</Text>
        </View>
      </Screen>
    );

  return (
    <Screen scroll>
      <View className="items-center gap-6">
        <View className="bg-white rounded-3xl p-6 shadow-sm items-center gap-4 w-full border border-gray-100">
          <Text className="text-xl font-bold text-gray-900 text-center">{ticket.concert.name}</Text>
          <Text className="text-gray-500 text-center">
            {dayjs(ticket.concert.startDate).format("ddd D MMM YYYY")}
          </Text>

          <View className="p-4 bg-white rounded-2xl border border-gray-200">
            <QRCode value={ticket.qrCode} size={200} />
          </View>

          <Text className="text-xs text-gray-400 font-mono">{ticket.id}</Text>
        </View>

        <View className="w-full gap-3">
          <Row label="Venue" value={ticket.concert.venueName} />
          <Row label="Artist" value={ticket.concert.artistName} />
          <Row label="Price" value={`£${ticket.concert.price.toFixed(2)}`} />
          <Row label="Purchased" value={dayjs(ticket.purchaseDate).format("D MMM YYYY, h:mm A")} />
          <Row label="Email" value={ticket.userEmail} />
        </View>
      </View>
    </Screen>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <View className="flex-row justify-between items-center py-2 border-b border-gray-100">
      <Text className="text-sm text-gray-500">{label}</Text>
      <Text className="text-sm font-medium text-gray-900">{value}</Text>
    </View>
  );
}
