import { MapPin } from "lucide-react";
import { GoogleMap } from "@/components/GoogleMap";

interface Props {
  town?: string;
  county?: string;
  lat?: number;
  lng?: number;
}

export function LocationSection({ town, county, lat, lng }: Readonly<Props>) {
  return (
    <div className="space-y-3">
      <h2 className="text-xl font-semibold">Location</h2>
      <p className="text-muted-foreground flex items-center gap-2">
        <MapPin className="size-4" />
        {[town, county].filter(Boolean).join(", ") || "No location set."}
      </p>
      {lat != null && lng != null && <GoogleMap className="mt-3" lat={lat} lng={lng} />}
    </div>
  );
}
