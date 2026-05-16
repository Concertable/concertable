import { MapPin } from "lucide-react";

interface Props {
  name: string;
  county?: string;
  town?: string;
}

export function VenueLocation({ name, county, town }: Readonly<Props>) {
  return (
    <div className="flex items-center gap-2">
      <MapPin className="size-4 shrink-0" />
      <div>
        <p className="font-medium">{name}</p>
        <p className="text-muted-foreground text-sm">
          {[county, town].filter(Boolean).join(", ")}
        </p>
      </div>
    </div>
  );
}
