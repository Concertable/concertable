import { useRef, type ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { useNavbarHeight } from "@/context/NavbarHeightContext";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";

interface Props {
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  onToggleEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
  actions?: ReactNode;
  canSave: boolean;
  error?: string | null;
}

export function ConfigBar({
  editMode,
  isDirty,
  isSaving,
  onToggleEdit,
  onSave,
  onCancel,
  actions,
  canSave,
  error,
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
      className="bg-background border-border sticky z-10 flex items-center justify-between gap-2 border-b px-6 py-3"
      style={{ top: navbarHeight }}
    >
      <div className="flex items-center gap-2">{actions}</div>
      <div className="flex items-center gap-2">
        {error && (
          <p className="text-destructive text-sm" data-testid="save-error">
            {error}
          </p>
        )}
        <Button
          variant={editMode ? "secondary" : "outline"}
          onClick={onToggleEdit}
          data-testid="edit"
        >
          {editMode ? "Editing" : "Edit"}
        </Button>
        <Button
          variant="outline"
          onClick={onCancel}
          disabled={!isDirty}
          data-testid="cancel"
        >
          Cancel
        </Button>
        <Button
          onClick={onSave}
          disabled={!isDirty || isSaving || !canSave}
          data-testid="save"
        >
          {isSaving ? "Saving..." : "Save"}
        </Button>
      </div>
    </div>
  );
}
