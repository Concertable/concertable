import { Textarea } from "@/components/ui/textarea";

interface EditableTextareaProps {
  value: string | undefined;
  onChange: (value: string) => void;
  editMode: boolean;
  placeholder?: string;
  className?: string;
}

export function EditableTextarea({
  value,
  onChange,
  editMode,
  placeholder = "—",
  className,
}: Readonly<EditableTextareaProps>) {
  if (editMode) {
    return (
      <Textarea
        value={value ?? ""}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        rows={4}
        className={className}
      />
    );
  }

  return (
    <p className={className}>
      {value || <span className="text-muted-foreground">{placeholder}</span>}
    </p>
  );
}
