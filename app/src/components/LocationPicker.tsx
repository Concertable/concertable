import { useEffect } from "react";
import usePlacesAutocomplete, { getGeocode, getLatLng } from "use-places-autocomplete";
import { Input } from "@/components/ui/input";
import type { LatLng } from "@/types/location";

interface Props {
  onSelect: (lat: number, lng: number) => void;
  latLng?: LatLng;
}

export function LocationPicker({ onSelect, latLng }: Readonly<Props>) {
  const {
    ready,
    value,
    suggestions: { status, data },
    setValue,
    clearSuggestions,
  } = usePlacesAutocomplete({
    requestOptions: {
      componentRestrictions: { country: "gb" },
      types: ["(cities)"],
    },
  });

  useEffect(() => {
    if (!latLng) return;

    async function resolveDisplayName() {
      const results = await getGeocode({ location: latLng });
      const locality = results.find(r => r.types.includes("locality") || r.types.includes("postal_town"));
      const display = locality ?? results[0];
      if (display?.formatted_address) {
        setValue(display.formatted_address, false);
      }
    }

    resolveDisplayName();
  }, [latLng?.lat, latLng?.lng]);

  async function handleSelect(description: string) {
    setValue(description, false);
    clearSuggestions();

    const results = await getGeocode({ address: description });
    const { lat, lng } = await getLatLng(results[0]);
    onSelect(lat, lng);
  }

  return (
    <div className="relative">
      <Input
        value={value}
        onChange={(e) => setValue(e.target.value)}
        disabled={!ready}
        placeholder="Location"
        className="border-none shadow-none focus-visible:ring-0 bg-transparent p-0 h-auto"
      />
      {status === "OK" && (
        <ul className="absolute z-10 w-full mt-1 bg-background border rounded-md shadow-md">
          {data.map(({ place_id, description }) => (
            <li
              key={place_id}
              onClick={() => handleSelect(description)}
              className="px-3 py-2 text-sm cursor-pointer hover:bg-muted"
            >
              {description}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
