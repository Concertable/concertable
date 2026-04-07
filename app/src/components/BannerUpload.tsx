import { Camera } from "lucide-react";
import { useScroll, useTransform, motion } from "framer-motion";
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

  const { scrollY } = useScroll();
  const y = useTransform(scrollY, [0, 300], ["0%", "-30%"]);

  return (
    <>
      <div className="absolute inset-0 overflow-hidden">
        <motion.div
          style={{ y }}
          className="absolute top-0 left-0 h-[150%] w-full"
        >
          <img
            src={src}
            alt={name}
            className="h-full w-full object-cover opacity-60"
          />
        </motion.div>
      </div>
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
