import { View } from "react-native";
import { MapPin } from "lucide-react-native";
import type { Venue } from "@concertable/shared/features/venues";
import type { ImageFile } from "@concertable/shared/types";
import { Text } from "@/components/ui/text";
import { Separator } from "@/components/ui/separator";
import { HeroMobile } from "../../../components/HeroMobile";
import { MapPicker } from "../../../components/MapPicker";
import { EditableTextarea } from "../../../components/editable/EditableTextarea";
import { theme } from "../../../lib/theme";

interface Props {
  venue: Venue;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
  onBannerChange?: (file: ImageFile) => void;
  onAvatarChange?: (file: ImageFile) => void;
  onLocationChange?: (
    lat: number,
    lng: number,
    county: string,
    town: string,
  ) => void;
}

export function VenueDetails({
  venue,
  onNameChange,
  onAboutChange,
  onBannerChange,
  onAvatarChange,
  onLocationChange,
}: Readonly<Props>) {
  const location = [venue.town, venue.county].filter(Boolean).join(", ");

  return (
    <View>
      <HeroMobile
        bannerUrl={venue.bannerUrl}
        avatar={venue.avatar}
        name={venue.name}
        town={venue.town}
        county={venue.county}
        namePlaceholder="Venue name"
        onNameChange={onNameChange}
        onBannerChange={onBannerChange}
        onAvatarChange={onAvatarChange}
      />

      <View className="px-4 py-6 gap-8">
        <Section title="About">
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell artists about your venue..."
          >
            {venue.about}
          </EditableTextarea>
        </Section>

        <Separator />

        <Section title="Location">
          <View className="flex-row items-center gap-2">
            <MapPin size={16} color={theme.mutedForeground} />
            <Text className="text-muted-foreground">
              {location || "No location set."}
            </Text>
          </View>
          <MapPicker
            latitude={venue.latitude}
            longitude={venue.longitude}
            onChange={onLocationChange}
          />
        </Section>

        <Separator />

        <Section title="Concerts">
          <Text className="text-muted-foreground">No upcoming concerts.</Text>
        </Section>

        <Separator />

        <Section title="Opportunities">
          <Text className="text-muted-foreground">No opportunities yet.</Text>
        </Section>
      </View>
    </View>
  );
}

function Section({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <View className="gap-3">
      <Text className="text-lg font-semibold text-foreground">{title}</Text>
      {children}
    </View>
  );
}
