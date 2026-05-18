import { useRef } from "react";
import { Button } from "@/components/ui/button";
import { useNavbarHeight } from "@/context/NavbarHeightContext";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";

interface Props {
  isSaving: boolean;
  canSubmit: boolean;
  onCreate: () => void;
}

export function CreateBar({ isSaving, canSubmit, onCreate }: Readonly<Props>) {
  const ref = useRef<HTMLDivElement>(null);
  const { navbarHeight, setConfigHeight } = useNavbarHeight();

  useMountLayoutEffect(() => {
    if (ref.current) setConfigHeight(ref.current.offsetHeight);
    return () => setConfigHeight(0);
  });

  return (
    <div
      ref={ref}
      className="bg-background border-border sticky z-10 flex items-center justify-end gap-2 border-b px-6 py-3"
      style={{ top: navbarHeight }}
    >
      <Button
        onClick={onCreate}
        disabled={!canSubmit || isSaving}
        data-testid="submit"
      >
        {isSaving ? "Creating..." : "Create"}
      </Button>
    </div>
  );
}
