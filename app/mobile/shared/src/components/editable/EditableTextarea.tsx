import { useState } from "react";
import { Text } from "@/components/ui/text";
import { Textarea } from "@/components/ui/textarea";
import { useEditableContext } from "@concertable/shared/providers";

interface Props {
  children?: string;
  onChange?: (value: string) => void;
  className?: string;
  placeholder?: string;
}

export function EditableTextarea({
  children,
  onChange,
  className,
  placeholder,
}: Readonly<Props>) {
  const editMode = useEditableContext();
  const [value, setValue] = useState(children ?? "");

  if (!editMode) {
    return <Text className={className}>{children}</Text>;
  }

  return (
    <Textarea
      value={value}
      onChangeText={(next) => {
        setValue(next);
        onChange?.(next);
      }}
      className={className}
      placeholder={placeholder}
    />
  );
}
