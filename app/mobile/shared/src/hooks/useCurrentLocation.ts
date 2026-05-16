import { useCallback, useState } from "react";
import { Alert } from "react-native";
import * as Location from "expo-location";
import googleGeocodingApi from "@concertable/shared/lib/googleGeocodingApi";

export interface ResolvedLocation {
  lat: number;
  lng: number;
  label?: string;
}

export function useCurrentLocation() {
  const [locating, setLocating] = useState(false);

  const requestLocation = useCallback(async (): Promise<ResolvedLocation | null> => {
    setLocating(true);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== "granted") {
        Alert.alert(
          "Location permission denied",
          "Enable location access in your device settings to use this feature.",
        );
        return null;
      }
      const loc = await Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
      const lat = loc.coords.latitude;
      const lng = loc.coords.longitude;
      const label = await googleGeocodingApi.reverseGeocode(lat, lng).catch(() => undefined);
      return { lat, lng, label };
    } catch {
      Alert.alert("Couldn't get location", "Please try again.");
      return null;
    } finally {
      setLocating(false);
    }
  }, []);

  return { requestLocation, locating };
}
