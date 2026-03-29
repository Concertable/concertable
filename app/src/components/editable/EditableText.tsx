import { Input } from "@/components/ui/input";
import type { TextElement } from "@/types/ui";
import { Editable } from "./Editable";

interface Props {
  value: string;
  onChange?: (value: string) => void;
  editMode?: boolean;
  className?: string;
  element: TextElement;
  placeholder?: string;
}

export function EditableText({
  value,
  onChange,
  className,
  element,
  placeholder,
}: Readonly<Props>) {
  const Tag = element;

  return (
    <Editable
      edit={
        <Input
          value={value}
          onChange={(e) => onChange?.(e.target.value)}
          className={className}
          placeholder={placeholder}
        />
      }
      view={<Tag className={className}>{value}</Tag>}
    />
  );
}