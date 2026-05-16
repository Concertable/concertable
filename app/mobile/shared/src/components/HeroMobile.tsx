import { Pressable, View } from "react-native";
import { Image } from "expo-image";
import { Camera, MapPin } from "lucide-react-native";
import * as ImagePicker from "expo-image-picker";
import { Text } from "@/components/ui/text";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useImageUrl } from "@concertable/shared/hooks";
import type { ImageFile } from "@concertable/shared/types";
import { EditableText } from "./editable/EditableText";
import { useEditableContext } from "@concertable/shared/providers";
import { theme } from "../lib/theme";

interface Props {
  bannerUrl?: string;
  avatar?: string;
  name: string;
  town?: string;
  county?: string;
  namePlaceholder?: string;
  onNameChange?: (value: string) => void;
  onBannerChange?: (file: ImageFile) => void;
  onAvatarChange?: (file: ImageFile) => void;
}

export function HeroMobile({
  bannerUrl,
  avatar,
  name,
  town,
  county,
  namePlaceholder,
  onNameChange,
  onBannerChange,
  onAvatarChange,
}: Readonly<Props>) {
  const editMode = useEditableContext();
  const { data: bannerSrc } = useImageUrl(bannerUrl);
  const { data: avatarSrc } = useImageUrl(avatar);

  const location = [town, county].filter(Boolean).join(", ");

  async function pick(onPicked?: (file: ImageFile) => void) {
    if (!onPicked) return;
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: "images",
      quality: 0.8,
      allowsEditing: true,
    });
    if (result.canceled) return;
    const asset = result.assets[0];
    onPicked({
      uri: asset.uri,
      name: asset.fileName ?? `image-${Date.now()}.jpg`,
      type: asset.mimeType ?? "image/jpeg",
    });
  }

  return (
    <View className="relative" style={{ height: 240 }}>
      <Pressable
        disabled={!editMode}
        onPress={() => pick(onBannerChange)}
        style={{ position: "absolute", inset: 0 }}
      >
        {bannerSrc ? (
          <Image
            source={{ uri: bannerSrc }}
            style={{ width: "100%", height: "100%" }}
            contentFit="cover"
          />
        ) : (
          <View className="bg-muted" style={{ width: "100%", height: "100%" }} />
        )}
        <View style={{ position: "absolute", inset: 0 }} className="bg-foreground/40" />
        {editMode && (
          <View className="absolute top-3 right-3 bg-background/80 rounded-full p-2">
            <Camera size={18} color={theme.foreground} />
          </View>
        )}
      </Pressable>

      <View className="absolute bottom-0 left-0 right-0 p-4 flex-row items-end gap-3">
        <Pressable
          disabled={!editMode}
          onPress={() => pick(onAvatarChange)}
        >
          <Avatar alt={name} className="w-20 h-20 border-2 border-background">
            {avatarSrc ? (
              <AvatarImage source={{ uri: avatarSrc }} />
            ) : (
              <AvatarFallback>
                <Text className="text-2xl font-semibold">
                  {name.charAt(0).toUpperCase()}
                </Text>
              </AvatarFallback>
            )}
          </Avatar>
          {editMode && (
            <View className="absolute bottom-0 right-0 bg-background rounded-full p-1.5 border border-border">
              <Camera size={12} color={theme.foreground} />
            </View>
          )}
        </Pressable>

        <View className="flex-1 gap-1">
          <EditableText
            onChange={onNameChange}
            placeholder={namePlaceholder}
            className="text-2xl font-bold text-white"
          >
            {name}
          </EditableText>
          {location.length > 0 && (
            <View className="flex-row items-center gap-1">
              <MapPin size={14} color="white" />
              <Text className="text-sm text-white/90">{location}</Text>
            </View>
          )}
        </View>
      </View>
    </View>
  );
}
