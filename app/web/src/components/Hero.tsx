import { MapPin } from "lucide-react";
import { EditableText } from "@/components/editable/EditableText";
import { BannerUpload } from "@/components/BannerUpload";
import { AvatarUpload } from "@/components/AvatarUpload";
import { useImageUrl } from "@/hooks/query/useImageUrl";
import { useBannerTextColor } from "@/hooks/useBannerTextColor";

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
  const { data: bannerSrc, isPending: bannerPending } = useImageUrl(bannerUrl);
  const textColor = useBannerTextColor(bannerSrc);

  return (
    <div className="bg-muted relative flex h-72 items-end">
      <BannerUpload
        src={bannerSrc}
        isPending={!!bannerUrl && bannerPending}
        name={name}
        onBannerChange={onBannerChange}
      />

      <div className="relative z-[5] flex w-full items-end justify-between gap-4 px-8 pb-6">
        <div className="space-y-1">
          <EditableText
            onChange={onNameChange}
            element="h1"
            placeholder={namePlaceholder}
            className={`text-3xl font-bold${textColor ? ` text-${textColor}` : ""}`}
          >
            {name}
          </EditableText>
          {(town || county) && (
            <p
              className={`flex items-center gap-1 text-sm${textColor ? ` text-${textColor}/80` : ""}`}
            >
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
