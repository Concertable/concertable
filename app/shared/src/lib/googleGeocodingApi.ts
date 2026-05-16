interface GeocodeComponent {
  long_name: string;
  short_name: string;
  types: string[];
}

interface GeocodeResult {
  address_components: GeocodeComponent[];
  formatted_address: string;
  types: string[];
}

interface GeocodeResponse {
  results: GeocodeResult[];
  status: string;
}

let apiKey = "";

function pickComponent(result: GeocodeResult, types: string[]): string | undefined {
  for (const t of types) {
    const match = result.address_components.find((c) => c.types.includes(t));
    if (match) return match.long_name;
  }
  return undefined;
}

function pickLocalityResult(results: GeocodeResult[]): GeocodeResult | undefined {
  return (
    results.find((r) => r.types.includes("postal_town")) ??
    results.find((r) => r.types.includes("locality")) ??
    results.find((r) => r.types.includes("administrative_area_level_2")) ??
    results[0]
  );
}

function formatLabel(result: GeocodeResult | undefined): string | undefined {
  if (!result) return undefined;
  const town = pickComponent(result, ["postal_town", "locality", "administrative_area_level_2"]);
  const area = pickComponent(result, ["administrative_area_level_1", "country"]);
  if (town && area && town !== area) return `${town}, ${area}`;
  return town ?? area;
}

const googleGeocodingApi = {
  configure: (key: string) => {
    apiKey = key;
  },

  reverseGeocode: async (lat: number, lng: number): Promise<string | undefined> => {
    if (!apiKey) throw new Error("Google geocoding API key not configured — call googleGeocodingApi.configure() at startup.");
    const url = `https://maps.googleapis.com/maps/api/geocode/json?latlng=${lat},${lng}&key=${apiKey}`;
    const res = await fetch(url);
    if (!res.ok) return undefined;
    const json = (await res.json()) as GeocodeResponse;
    if (json.status !== "OK") return undefined;
    return formatLabel(pickLocalityResult(json.results));
  },
};

export default googleGeocodingApi;
