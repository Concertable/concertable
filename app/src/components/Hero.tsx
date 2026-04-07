import { MapPin } from "lucide-react";
import { EditableText } from "@/components/editable/EditableText";
import { BannerUpload } from "@/components/BannerUpload";
import { AvatarUpload } from "@/components/AvatarUpload";

interface Props {
  bannerUrl?: string;
  avatar?: string;
  name: string;
  town?: string;
  county?: string;
  namePlaceholder?: string;
  onNameChange?: (value: string) => void;
  onBannerChange?: (file: File) => void;
  onAvatarChange?: (file: File) => void;
}

export function Hero({
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
  return (
    <div className="bg-muted relative flex h-72 items-end">
      <BannerUpload
        bannerUrl={bannerUrl}
        name={name}
        onBannerChange={onBannerChange}
      />

      <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />

      <div className="relative z-10 flex w-full items-end justify-between gap-4 px-8 pb-6">
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

        <AvatarUpload
          avatar={avatar}
          name={name}
          onAvatarChange={onAvatarChange}
        />
      </div>
    </div>
  );
}
