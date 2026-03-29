import { Input } from "@/components/ui/input";

interface EditableTextProps {
  value: string | undefined;
  onChange: (value: string) => void;
  editMode: boolean;
  as?: "h1" | "h2" | "h3" | "p";
  placeholder?: string;
  className?: string;
}

export function EditableText({
  value,
  onChange,
  editMode,
  as: Tag = "p",
  placeholder = "—",
  className,
}: Readonly<EditableTextProps>) {
  if (editMode) {
    return (
      <Input
        value={value ?? ""}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className={className}
      />
    );
  }

  return (
    <Tag className={className}>
      {value || <span className="text-muted-foreground">{placeholder}</span>}
    </Tag>
  );
}
