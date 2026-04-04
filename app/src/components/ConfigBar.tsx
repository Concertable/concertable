import { useRef } from "react";
import { Button } from "@/components/ui/button";
import { useNavbarHeight } from "@/context/NavbarHeightContext";
import { useMountLayoutEffect } from "@/hooks/useMountLayoutEffect";

interface ConfigBarProps {
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  onToggleEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
}

export function ConfigBar({
  editMode,
  isDirty,
  isSaving,
  onToggleEdit,
  onSave,
  onCancel,
}: Readonly<ConfigBarProps>) {
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
        variant={editMode ? "secondary" : "outline"}
        onClick={onToggleEdit}
      >
        {editMode ? "Editing" : "Edit"}
      </Button>
      <Button variant="outline" onClick={onCancel} disabled={!isDirty}>
        Cancel
      </Button>
      <Button onClick={onSave} disabled={!isDirty || isSaving}>
        {isSaving ? "Saving..." : "Save"}
      </Button>
    </div>
  );
}
