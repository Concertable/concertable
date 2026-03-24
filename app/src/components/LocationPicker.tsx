import usePlacesAutocomplete, { getGeocode, getLatLng } from "use-places-autocomplete";
import { Input } from "@/components/ui/input";
import type { LatLng } from "@/types/location";

interface Props {
  onSelect: (location: LatLng) => void;
}

export function LocationPicker({ onSelect }: Props) {
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

  async function handleSelect(description: string) {
    setValue(description, false);
    clearSuggestions();

    const results = await getGeocode({ address: description });
    const { lat, lng } = await getLatLng(results[0]);
    onSelect({ lat, lng });
  }

  return (
    <div className="relative">
      <Input
        value={value}
        onChange={(e) => setValue(e.target.value)}
        disabled={!ready}
        placeholder="Location"
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
