import { Camera } from "lucide-react";
import { useEditableContext } from "@/providers/EditableProvider";
import { useImageUpload } from "@/hooks/useImageUpload";
import { useImageUrl } from "@/hooks/query/useImageUrl";

interface Props {
  bannerUrl?: string;
  name: string;
  onBannerChange?: (file: File) => void;
}

export function BannerUpload({
  bannerUrl,
  name,
  onBannerChange,
}: Readonly<Props>) {
  const editMode = useEditableContext();
  const { inputRef, open, onChange } = useImageUpload(onBannerChange);
  const { data: src } = useImageUrl(bannerUrl);

  return (
    <>
      <img
        src={src}
        alt={name}
        className="absolute inset-0 h-full w-full object-cover opacity-60"
      />
      {editMode && (
        <>
          <button
            type="button"
            onClick={open}
            className="absolute top-3 right-3 z-10 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
          >
            <Camera className="size-4" />
          </button>
          <input
            ref={inputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={onChange}
          />
        </>
      )}
    </>
  );
}
