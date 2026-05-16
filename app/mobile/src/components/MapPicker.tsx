import { View } from "react-native";
import MapView, { Marker, PROVIDER_GOOGLE, type MapPressEvent } from "react-native-maps";
import * as Location from "expo-location";
import { useEditableContext } from "@concertable/shared/providers";

interface Props {
  latitude?: number;
  longitude?: number;
  onChange?: (lat: number, lng: number, county: string, town: string) => void;
  height?: number;
}

export function MapPicker({
  latitude,
  longitude,
  onChange,
  height = 200,
}: Readonly<Props>) {
  const editMode = useEditableContext();
  const hasCoords = typeof latitude === "number" && typeof longitude === "number";
  const lat = hasCoords ? latitude! : 54.6;
  const lng = hasCoords ? longitude! : -5.9;

  async function commit(nextLat: number, nextLng: number) {
    if (!onChange) return;
    let town = "";
    let county = "";
    try {
      const results = await Location.reverseGeocodeAsync({
        latitude: nextLat,
        longitude: nextLng,
      });
      const first = results[0];
      if (first) {
        town = first.city ?? first.name ?? "";
        county = first.subregion ?? first.region ?? "";
      }
    } catch {
      // best-effort; coords still propagate
    }
    onChange(nextLat, nextLng, county, town);
  }

  function handlePress(e: MapPressEvent) {
    if (!editMode) return;
    const { latitude: nLat, longitude: nLng } = e.nativeEvent.coordinate;
    commit(nLat, nLng);
  }

  return (
    <View style={{ height }} className="rounded-md overflow-hidden border border-border">
      <MapView
        provider={PROVIDER_GOOGLE}
        style={{ flex: 1 }}
        initialRegion={{
          latitude: lat,
          longitude: lng,
          latitudeDelta: 0.1,
          longitudeDelta: 0.1,
        }}
        onPress={handlePress}
        scrollEnabled={editMode}
        zoomEnabled
      >
        {hasCoords && (
          <Marker
            coordinate={{ latitude: lat, longitude: lng }}
            draggable={editMode}
            onDragEnd={(e) => {
              const { latitude: nLat, longitude: nLng } = e.nativeEvent.coordinate;
              commit(nLat, nLng);
            }}
          />
        )}
      </MapView>
    </View>
  );
}
