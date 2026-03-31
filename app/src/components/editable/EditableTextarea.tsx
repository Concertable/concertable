import { useDebounce } from "@/hooks/useDebounce";
import { Editable } from "./Editable";
import { Textarea } from "@/components/ui/textarea";

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
  const debouncedOnChange = useDebounce(onChange, 300);

  return (
    <Editable
      view={<p className={className}>{children}</p>}
      edit={
        <Textarea
          defaultValue={children}
          onChange={(e) => debouncedOnChange?.(e.target.value)}
          className={className}
          placeholder={placeholder}
        />
      }
    />
  );
}
