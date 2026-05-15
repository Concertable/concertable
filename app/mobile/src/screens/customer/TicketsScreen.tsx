import { useState } from "react";
import { FlatList, Pressable, Text, View, ActivityIndicator } from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import {
  useUpcomingTicketsQuery,
  useTicketHistoryQuery,
} from "@concertable/shared/features/concerts";
import type { Ticket } from "@concertable/shared/features/concerts";
import { Screen } from "../../components/ui/Screen";
import { Card } from "../../components/ui/Card";
import { Badge } from "../../components/ui/Badge";
import dayjs from "dayjs";
import type { TicketsStackParamList } from "../../navigation/types";

type Props = NativeStackScreenProps<TicketsStackParamList, "TicketsMain">;

export function TicketsScreen({ navigation }: Props) {
  const [tab, setTab] = useState<"upcoming" | "history">("upcoming");
  const { data: upcoming, isLoading: upLoading } = useUpcomingTicketsQuery();
  const { data: history, isLoading: histLoading } = useTicketHistoryQuery();

  const tickets = tab === "upcoming" ? upcoming : history;
  const isLoading = tab === "upcoming" ? upLoading : histLoading;

  return (
    <Screen>
      <View className="flex-row rounded-xl overflow-hidden border border-gray-200 mb-4">
        {(["upcoming", "history"] as const).map((t) => (
          <Pressable
            key={t}
            onPress={() => setTab(t)}
            className={`flex-1 py-2.5 items-center ${tab === t ? "bg-black" : "bg-white"}`}
          >
            <Text className={`font-semibold ${tab === t ? "text-white" : "text-gray-600"}`}>
              {t === "upcoming" ? "Upcoming" : "History"}
            </Text>
          </Pressable>
        ))}
      </View>

      {isLoading ? (
        <View className="flex-1 items-center justify-center">
          <ActivityIndicator size="large" />
        </View>
      ) : (
        <FlatList
          data={tickets}
          keyExtractor={(item) => item.id}
          showsVerticalScrollIndicator={false}
          contentContainerClassName="gap-3 pb-4"
          renderItem={({ item }) => (
            <TicketCard
              ticket={item}
              onPress={() => navigation.navigate("TicketDetail", { ticketId: item.id })}
            />
          )}
          ListEmptyComponent={
            <Text className="text-gray-500 text-center mt-12">No tickets here.</Text>
          }
        />
      )}
    </Screen>
  );
}

function TicketCard({ ticket, onPress }: { ticket: Ticket; onPress: () => void }) {
  const isUpcoming = dayjs(ticket.concert.startDate).isAfter(dayjs());
  return (
    <Pressable onPress={onPress}>
      <Card className="gap-2">
        <View className="flex-row items-start justify-between">
          <Text className="font-semibold text-gray-900 flex-1 mr-2" numberOfLines={1}>
            {ticket.concert.name}
          </Text>
          <Badge variant={isUpcoming ? "default" : "secondary"}>
            {isUpcoming ? "Upcoming" : "Past"}
          </Badge>
        </View>
        <Text className="text-sm text-gray-500">{ticket.concert.venueName}</Text>
        <Text className="text-sm text-gray-500">
          {dayjs(ticket.concert.startDate).format("ddd D MMM YYYY")}
        </Text>
        <Text className="text-xs text-gray-400">
          Artist: {ticket.concert.artistName} · £{ticket.concert.price.toFixed(2)}
        </Text>
      </Card>
    </Pressable>
  );
}
