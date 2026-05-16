import { View } from "react-native";
import { MapPin } from "lucide-react-native";
import type { Artist } from "@concertable/shared/features/artists";
import type { ImageFile } from "@concertable/shared/types";
import { Text } from "@/components/ui/text";
import { Separator } from "@/components/ui/separator";
import { HeroMobile } from "../../../components/HeroMobile";
import { EditableTextarea } from "../../../components/editable/EditableTextarea";
import { theme } from "../../../lib/theme";

interface Props {
  artist: Artist;
  onNameChange?: (value: string) => void;
  onAboutChange?: (value: string) => void;
  onBannerChange?: (file: ImageFile) => void;
  onAvatarChange?: (file: ImageFile) => void;
}

export function ArtistDetails({
  artist,
  onNameChange,
  onAboutChange,
  onBannerChange,
  onAvatarChange,
}: Readonly<Props>) {
  const location = [artist.town, artist.county].filter(Boolean).join(", ");

  return (
    <View>
      <HeroMobile
        bannerUrl={artist.bannerUrl}
        avatar={artist.avatar}
        name={artist.name}
        town={artist.town}
        county={artist.county}
        namePlaceholder="Artist name"
        onNameChange={onNameChange}
        onBannerChange={onBannerChange}
        onAvatarChange={onAvatarChange}
      />

      <View className="px-4 py-6 gap-8">
        <Section title="About">
          <EditableTextarea
            onChange={onAboutChange}
            placeholder="Tell venues about yourself..."
          >
            {artist.about}
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
