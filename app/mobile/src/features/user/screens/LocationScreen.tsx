import { useRef, useState } from "react";
import { Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import MapView, { Marker, type Region } from "react-native-maps";
import * as Location from "expo-location";
import { MapPin } from "lucide-react-native";
import { useNavigation } from "@react-navigation/native";
import { useUserQuery, useUpdateLocationMutation } from "@concertable/shared/features/user";
import { Button } from "../../../components/ui/Button";
import { Skeleton } from "../../../components/ui/Skeleton";
import { notify } from "../../../lib/toast";

const UK_DEFAULT: Region = {
  latitude: 51.5074,
  longitude: -0.1278,
  latitudeDelta: 5,
  longitudeDelta: 5,
};

export function LocationScreen() {
  const nav = useNavigation();
  const { data: user, isLoading } = useUserQuery();
  const updateLocation = useUpdateLocationMutation();

  const hasUserLocation = !!(user?.latitude && user?.longitude);

  const [coordinate, setCoordinate] = useState<{ latitude: number; longitude: number } | null>(
    hasUserLocation ? { latitude: user!.latitude!, longitude: user!.longitude! } : null,
  );
  const [locating, setLocating] = useState(false);
  const mapRef = useRef<MapView>(null);

  async function handleUseMyLocation() {
    setLocating(true);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== "granted") {
        notify("Location permission denied", "error");
        return;
      }
      const pos = await Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
      const { latitude, longitude } = pos.coords;
      setCoordinate({ latitude, longitude });
      mapRef.current?.animateToRegion({ latitude, longitude, latitudeDelta: 0.1, longitudeDelta: 0.1 }, 400);
    } catch {
      notify("Could not get your location", "error");
    } finally {
      setLocating(false);
    }
  }

  async function handleSave() {
    if (!coordinate) {
      notify("Pin a location on the map first", "error");
      return;
    }
    try {
      await updateLocation.mutateAsync(coordinate);
      notify("Location saved", "success");
      nav.goBack();
    } catch {
      notify("Failed to save location", "error");
    }
  }

  if (isLoading) {
    return (
      <View className="flex-1 bg-background p-4 gap-4">
        <Skeleton width="100%" height={300} className="rounded-xl" />
        <Skeleton width="100%" height={48} className="rounded-xl" />
      </View>
    );
  }

  const initialRegion: Region = coordinate
    ? { ...coordinate, latitudeDelta: 0.1, longitudeDelta: 0.1 }
    : UK_DEFAULT;

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <View className="flex-1">
        <View style={{ borderRadius: 0, overflow: "hidden", flex: 1 }}>
          <MapView
            ref={mapRef}
            style={{ flex: 1 }}
            initialRegion={initialRegion}
            onPress={(e) => setCoordinate(e.nativeEvent.coordinate)}
          >
            {coordinate && (
              <Marker
                coordinate={coordinate}
                draggable
                onDragEnd={(e) => setCoordinate(e.nativeEvent.coordinate)}
              />
            )}
          </MapView>
        </View>

        <View className="px-4 pt-3 pb-6 gap-3 bg-background border-t border-border">
          <View className="flex-row items-center gap-1.5">
            <MapPin size={14} className="text-muted-foreground" />
            <Text className="text-xs text-muted-foreground">
              {coordinate
                ? `${coordinate.latitude.toFixed(5)}, ${coordinate.longitude.toFixed(5)}`
                : "Tap the map to set your location"}
            </Text>
          </View>
          <View className="flex-row gap-2">
            <View className="flex-1">
              <Button variant="outline" loading={locating} onPress={handleUseMyLocation}>
                Use My Location
              </Button>
            </View>
            <View className="flex-1">
              <Button loading={updateLocation.isPending} onPress={handleSave}>
                Save
              </Button>
            </View>
          </View>
        </View>
      </View>
    </SafeAreaView>
  );
}
