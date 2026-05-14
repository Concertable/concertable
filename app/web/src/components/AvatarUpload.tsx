import { Camera } from "lucide-react";
import { useEditableContext } from "@/providers/EditableProvider";
import { useImageUpload } from "@/hooks/useImageUpload";
import { useImageUrl } from "@/hooks/query/useImageUrl";

interface Props {
  avatar?: string;
  name: string;
  onAvatarChange?: (file: File) => void;
}

function AvatarSkeleton() {
  return (
    <div className="h-32 w-32 animate-pulse rounded-lg border-2 border-white bg-white/20" />
  );
}

export function AvatarUpload({
  avatar,
  name,
  onAvatarChange,
}: Readonly<Props>) {
  const editMode = useEditableContext();
  const { inputRef, open, onChange } = useImageUpload(onAvatarChange);
  const { data: src, isPending } = useImageUrl(avatar);

  return (
    <div className="relative z-0 shrink-0">
      {isPending && avatar ? (
        <AvatarSkeleton />
      ) : (
        <img
          src={src}
          alt={name}
          className="h-32 w-32 rounded-lg border-2 border-white object-cover"
        />
      )}
      {editMode && (
        <>
          <button
            type="button"
            onClick={open}
            className="absolute inset-0 flex cursor-pointer items-center justify-center rounded-lg bg-black/50 text-white opacity-0 hover:opacity-100"
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
    </div>
  );
}
