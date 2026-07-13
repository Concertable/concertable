import { MapPin } from "lucide-react";
import type { ImageFile } from "@concertable/shared";
import { EditableText } from "@/components/editable/EditableText";
import { BannerUpload } from "@/components/BannerUpload";
import { AvatarUpload } from "@/components/AvatarUpload";
import { useImageUrl } from "@concertable/shared/hooks";

interface Props {
  bannerUrl?: string;
  avatar?: string;
  name: string;
  town?: string;
  county?: string;
  namePlaceholder?: string;
  onNameChange?: (value: string) => void;
  onBannerChange?: (file: ImageFile) => void;
  onAvatarChange?: (file: ImageFile) => void;
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

  return (
    <div className="bg-muted relative flex h-72 items-end">
      <BannerUpload
        src={bannerSrc}
        isPending={!!bannerUrl && bannerPending}
        name={name}
        onBannerChange={onBannerChange}
        testId="hero-banner"
      />

      <div className="pointer-events-none absolute inset-0 bg-gradient-to-t from-black/70 via-black/25 to-transparent" />

      <div className="relative z-[5] flex w-full items-end justify-between gap-4 px-8 pb-6 text-white">
        <div className="space-y-1 [text-shadow:0_1px_3px_rgb(0_0_0/0.4)]">
          <EditableText
            onChange={onNameChange}
            element="h1"
            placeholder={namePlaceholder}
            testId="hero-name"
            className="text-3xl font-bold"
          >
            {name}
          </EditableText>
          {(town || county) && (
            <p className="flex items-center gap-1 text-sm text-white/85">
              <MapPin className="size-4" />
              {[town, county].filter(Boolean).join(", ")}
            </p>
          )}
        </div>

        <AvatarUpload
          avatar={avatar}
          name={name}
          onAvatarChange={onAvatarChange}
          testId="hero-avatar"
        />
      </div>
    </div>
  );
}
