import { useState } from "react";

export function useSearchState() {
  const [open, setOpen] = useState(false);

  return {
    open,
    close: () => setOpen(false),
    inputProps: {
      onFocus: () => setOpen(true),
      onBlur: () => setOpen(false),
    },
  };
}
