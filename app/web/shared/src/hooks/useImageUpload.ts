import { useRef } from "react";
import type { ImageFile } from "@concertable/shared";

export function useImageUpload(onFileSelected?: (file: ImageFile) => void) {
  const inputRef = useRef<HTMLInputElement>(null);

  function open() {
    inputRef.current?.click();
  }

  function onChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) onFileSelected?.(Object.assign(file, { uri: URL.createObjectURL(file) }));
  }

  return { inputRef, open, onChange };
}
