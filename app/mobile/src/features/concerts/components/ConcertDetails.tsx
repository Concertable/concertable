import { useState } from "react";
import { ScrollView, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import MapView, { Marker } from "react-native-maps";
import { Image } from "expo-image";
import { CalendarDays, MapPin, Music, TriangleAlert } from "lucide-react-native";
import type { Concert } from "@concertable/shared/features/concerts";
import { Hero } from "../../../components/ui/Hero";
import { RatingStars } from "../../../components/ui/RatingStars";
import { GenreChips } from "../../../components/ui/GenreChips";
import { EmptyState } from "../../../components/ui/EmptyState";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { theme } from "../../../lib/theme";
import dayjs from "dayjs";
import type { ConcertNavParamList } from "../../../navigation/types";

type ConcertNav = NativeStackNavigationProp<ConcertNavParamList>;

const SECTION_OPTIONS = [
  { value: "about" as const, label: "About" },
  { value: "artist" as const, label: "Artist" },
  { value: "venue" as const, label: "Venue" },
  { value: "reviews" as const, label: "Reviews" },
];
type SectionKey = "about" | "artist" | "venue" | "reviews";

interface Props {
  concert: Concert;
}

export function ConcertDetails({ concert }: Readonly<Props>) {
  const nav = useNavigation<ConcertNav>();
  const [section, setSection] = useState<SectionKey>("about");
  const soldOut = concert.availableTickets === 0;

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <ScrollView showsVerticalScrollIndicator={false}>
        <Hero
          uri={concert.bannerUrl}
          title={concert.name}
          subtitle={`${concert.venue.town}, ${concert.venue.county}`}
          trailing={
            concert.artist.avatar ? (
              <Image
                source={{ uri: concert.artist.avatar }}
                style={{
                  width: 48,
                  height: 48,
                  borderRadius: 24,
                  borderWidth: 2,
                  borderColor: "rgba(255,255,255,0.7)",
                }}
                contentFit="cover"
              />
            ) : undefined
          }
        />

        {!concert.datePosted && (
          <View className="flex-row items-center gap-2 mx-4 mt-4 px-3 py-2.5 rounded-xl bg-warning/15">
            <TriangleAlert size={14} color={theme.warning} />
            <Text className="text-xs font-medium text-warning">
              This concert hasn't been posted yet
            </Text>
          </View>
        )}

        <View className="px-4 pt-5 pb-2">
          <Tabs value={section} onValueChange={(v) => setSection(v as SectionKey)}>
            <TabsList className="w-full">
              {SECTION_OPTIONS.map((opt) => (
                <TabsTrigger key={opt.value} value={opt.value} className="flex-1">
                  <Text>{opt.label}</Text>
                </TabsTrigger>
              ))}
            </TabsList>
          </Tabs>
        </View>

        {section === "about" && <AboutSection concert={concert} />}
        {section === "artist" && <ArtistSection concert={concert} />}
        {section === "venue" && <VenueSection concert={concert} />}
        {section === "reviews" && <ReviewsSection />}
      </ScrollView>

      <View className="px-4 pt-3 pb-4 border-t border-border bg-card">
        <View className="flex-row items-center justify-between mb-3">
          <View>
            <Text className="text-2xl font-bold text-foreground">£{concert.price.toFixed(2)}</Text>
            <Text className="text-xs text-muted-foreground">
              {soldOut
                ? "Sold out"
                : `${concert.availableTickets} ticket${concert.availableTickets !== 1 ? "s" : ""} left`}
            </Text>
          </View>
          {concert.rating != null && <RatingStars rating={concert.rating} size={16} />}
        </View>
        <Button
          disabled={soldOut}
          size="lg"
          onPress={() => nav.navigate("TicketCheckout", { concertId: concert.id })}
        >
          <Text>{soldOut ? "Sold Out" : "Buy Tickets"}</Text>
        </Button>
      </View>
    </SafeAreaView>
  );
}

function AboutSection({ concert }: { concert: Concert }) {
  return (
    <View className="px-4 py-5 gap-4">
      <View className="flex-row items-start gap-2">
        <CalendarDays size={16} color={theme.primary} />
        <View>
          <Text className="text-sm font-medium text-foreground">
            {dayjs(concert.startDate).format("ddd D MMM YYYY")}
          </Text>
          {concert.endDate !== concert.startDate && (
            <Text className="text-xs text-muted-foreground">
              until {dayjs(concert.endDate).format("ddd D MMM YYYY")}
            </Text>
          )}
        </View>
      </View>

      <View className="flex-row items-center gap-2">
        <MapPin size={16} color={theme.primary} />
        <Text className="text-sm text-foreground">
          {concert.venue.town}, {concert.venue.county}
        </Text>
      </View>

      {concert.genres.length > 0 && <GenreChips genres={concert.genres} />}

      {concert.about ? (
        <Text className="text-sm text-foreground leading-relaxed">{concert.about}</Text>
      ) : (
        <Text className="text-sm text-muted-foreground italic">No description provided.</Text>
      )}
    </View>
  );
}

function ArtistSection({ concert }: { concert: Concert }) {
  return (
    <View className="px-4 py-5 gap-4">
      <View className="flex-row items-center gap-4">
        {concert.artist.avatar ? (
          <Image
            source={{ uri: concert.artist.avatar }}
            style={{ width: 64, height: 64, borderRadius: 32 }}
            contentFit="cover"
          />
        ) : (
          <View className="w-16 h-16 rounded-full bg-muted items-center justify-center">
            <Music size={24} color={theme.mutedForeground} />
          </View>
        )}
        <View className="flex-1 gap-1">
          <Text className="text-lg font-semibold text-foreground">{concert.artist.name}</Text>
          <View className="flex-row items-center gap-1">
            <MapPin size={12} color={theme.mutedForeground} />
            <Text className="text-xs text-muted-foreground">
              {concert.artist.town}, {concert.artist.county}
            </Text>
          </View>
          {concert.artist.rating != null && <RatingStars rating={concert.artist.rating} />}
        </View>
      </View>
      {concert.artist.genres.length > 0 && <GenreChips genres={concert.artist.genres} />}
    </View>
  );
}

function VenueSection({ concert }: { concert: Concert }) {
  const hasCoords = concert.venue.latitude != null && concert.venue.longitude != null;
  return (
    <View className="px-4 py-5 gap-4">
      <View>
        <Text className="text-lg font-semibold text-foreground">{concert.venue.name}</Text>
        <View className="flex-row items-center gap-1 mt-1">
          <MapPin size={12} color={theme.mutedForeground} />
          <Text className="text-sm text-muted-foreground">
            {concert.venue.town}, {concert.venue.county}
          </Text>
        </View>
      </View>
      {hasCoords && (
        <View style={{ borderRadius: 12, overflow: "hidden" }}>
          <MapView
            style={{ height: 200 }}
            initialRegion={{
              latitude: concert.venue.latitude,
              longitude: concert.venue.longitude,
              latitudeDelta: 0.01,
              longitudeDelta: 0.01,
            }}
            scrollEnabled={false}
            zoomEnabled={false}
          >
            <Marker
              coordinate={{
                latitude: concert.venue.latitude,
                longitude: concert.venue.longitude,
              }}
              pinColor={theme.primary}
            />
          </MapView>
        </View>
      )}
    </View>
  );
}

function ReviewsSection() {
  return (
    <EmptyState
      title="No reviews yet"
      description="Reviews will appear here after the concert"
      className="py-16"
    />
  );
}
