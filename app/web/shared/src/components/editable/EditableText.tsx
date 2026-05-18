import type { TextElement } from "@/types/ui";
import { useDebounce } from "@/hooks/useDebounce";
import { Editable } from "./Editable";
import { Input } from "../ui/input";

interface Props {
  children?: string;
  onChange?: (value: string) => void;
  className?: string;
  element: TextElement;
  placeholder?: string;
  testId?: string;
}

export function EditableText({
  children,
  onChange,
  className,
  element,
  placeholder,
  testId,
}: Readonly<Props>) {
  const Tag = element;
  const debouncedOnChange = useDebounce(onChange, 300);

  return (
    <Editable
      view={<Tag className={className}>{children}</Tag>}
      edit={
        <Input
          defaultValue={children}
          onChange={(e) => debouncedOnChange?.(e.target.value)}
          className={className}
          placeholder={placeholder}
          data-testid={testId}
        />
      }
    />
  );
}
