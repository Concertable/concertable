import { useState } from "react";
import { Text } from "@/components/ui/text";
import { Input } from "@/components/ui/input";
import { useEditableContext } from "@concertable/shared/providers";

interface Props {
  children?: string;
  onChange?: (value: string) => void;
  className?: string;
  placeholder?: string;
}

export function EditableText({
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
    <Input
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
