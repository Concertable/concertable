import { Camera } from "lucide-react";
import { useScroll, useTransform, motion } from "framer-motion";
import { useEditableContext } from "@/providers/EditableProvider";
import { useImageUpload } from "@/hooks/useImageUpload";

interface Props {
  src?: string;
  isPending?: boolean;
  name: string;
  onBannerChange?: (file: File) => void;
}

function BannerSkeleton() {
  return <div className="absolute inset-0 animate-pulse bg-white/10" />;
}

export function BannerUpload({
  src,
  isPending,
  name,
  onBannerChange,
}: Readonly<Props>) {
  const editMode = useEditableContext();
  const { inputRef, open, onChange } = useImageUpload(onBannerChange);

  const { scrollY } = useScroll();
  const y = useTransform(scrollY, [0, 300], ["0%", "-30%"]);

  return (
    <>
      {isPending ? (
        <BannerSkeleton />
      ) : (
        <div className="absolute inset-0 overflow-hidden">
          <motion.div
            style={{ y }}
            className="absolute top-0 left-0 h-[150%] w-full"
          >
            <img src={src} alt={name} className="h-full w-full object-cover" />
          </motion.div>
        </div>
      )}
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
