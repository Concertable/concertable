import { useRef } from "react";
import { Button } from "@/components/ui/button";
import { useNavbarHeight } from "@/context/NavbarHeightContext";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";

interface Props {
  isSaving: boolean;
  canSubmit: boolean;
  error?: string;
  onCreate: () => void;
}

export function CreateBar({
  isSaving,
  canSubmit,
  error,
  onCreate,
}: Readonly<Props>) {
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
      <div className="flex items-center gap-2">
        {error && (
          <p className="text-destructive text-sm" data-testid="create-error">
            {error}
          </p>
        )}
        <Button
          onClick={onCreate}
          disabled={!canSubmit || isSaving}
          data-testid="submit"
        >
          {isSaving ? "Creating..." : "Create"}
        </Button>
      </div>
    </div>
  );
}
