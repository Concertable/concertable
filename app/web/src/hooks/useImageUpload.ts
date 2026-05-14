import { useRef } from "react";

export function useImageUpload(onFileSelected?: (file: File) => void) {
  const inputRef = useRef<HTMLInputElement>(null);

  function open() {
    inputRef.current?.click();
  }

  function onChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) onFileSelected?.(file);
  }

  return { inputRef, open, onChange };
}
