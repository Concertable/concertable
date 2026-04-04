import { MapPin, Star } from "lucide-react";
import { EditableText } from "@/components/editable/EditableText";

interface Props {
  imageUrl?: string;
  name: string;
  town?: string;
  county?: string;
  namePlaceholder?: string;
  onNameChange?: (value: string) => void;
}

export function Hero({
  imageUrl,
  name,
  town,
  county,
  namePlaceholder,
  onNameChange,
}: Readonly<Props>) {
  return (
    <div className="bg-muted relative flex h-72 items-end">
      {imageUrl && (
        <img
          src={imageUrl}
          alt={name}
          className="absolute inset-0 h-full w-full object-cover opacity-60"
        />
      )}
      <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
      <div className="relative z-10 flex w-full items-end justify-between px-8 pb-6">
        <div className="space-y-1">
          <EditableText
            onChange={onNameChange}
            element="h1"
            placeholder={namePlaceholder}
            className="text-3xl font-bold text-white drop-shadow"
          >
            {name}
          </EditableText>
          {(town || county) && (
            <p className="flex items-center gap-1 text-sm text-white/80 drop-shadow">
              <MapPin className="size-4" />
              {[town, county].filter(Boolean).join(", ")}
            </p>
          )}
        </div>
        <div className="flex items-center gap-1 text-sm text-white/80 drop-shadow">
          <Star className="fill-gold text-gold size-4" />
          <span>No reviews yet</span>
        </div>
      </div>
    </div>
  );
}
