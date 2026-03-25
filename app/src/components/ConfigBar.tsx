import { Button } from "@/components/ui/button";

interface ConfigBarProps {
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  onToggleEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
}

export function ConfigBar({ editMode, isDirty, isSaving, onToggleEdit, onSave, onCancel }: ConfigBarProps) {
  return (
    <div className="flex items-center justify-end gap-2 border-b border-border px-6 py-3">
      <Button variant={editMode ? "secondary" : "outline"} onClick={onToggleEdit}>
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
