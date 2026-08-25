import { ActivityIndicator, View } from "react-native";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";

interface Props {
  editMode: boolean;
  isDirty: boolean;
  isSaving: boolean;
  canSave?: boolean;
  error?: string;
  onToggleEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
}

export function ConfigBar({
  editMode,
  isDirty,
  isSaving,
  canSave = true,
  error,
  onToggleEdit,
  onSave,
  onCancel,
}: Readonly<Props>) {
  if (!editMode) {
    return (
      <Button variant="secondary" size="sm" onPress={onToggleEdit}>
        <Text>Edit</Text>
      </Button>
    );
  }

  return (
    <View className="flex-row items-center gap-2">
      {error && <Text className="text-destructive text-xs">{error}</Text>}
      <Button variant="outline" size="sm" onPress={onCancel} disabled={isSaving}>
        <Text>Cancel</Text>
      </Button>
      <Button
        size="sm"
        onPress={onSave}
        disabled={!isDirty || isSaving || !canSave}
      >
        {isSaving ? (
          <ActivityIndicator size="small" color="white" />
        ) : (
          <Text>Save</Text>
        )}
      </Button>
    </View>
  );
}
