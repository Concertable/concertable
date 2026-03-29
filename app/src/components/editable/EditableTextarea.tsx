import { Textarea } from "@/components/ui/textarea";
import { Editable } from "./Editable";

interface Props {
  value: string | undefined;
  onChange?: (value: string) => void;
  className?: string;
  placeholder?: string;
}

export function EditableTextarea({
  value,
  onChange,
  className,
  placeholder,
}: Readonly<Props>) {
  return (
    <Editable
      edit={
        <Textarea
          value={value}
          onChange={(e) => onChange?.(e.target.value)}
          className={className}
          placeholder={placeholder}
        />
      }
      view={<p className={className}>{value}</p>}
    />
  );
}
