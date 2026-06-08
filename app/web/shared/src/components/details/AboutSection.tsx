import { EditableTextarea } from "@/components/editable/EditableTextarea";

interface Props {
  text?: string;
  placeholder?: string;
  onChange?: (value: string) => void;
}

export function AboutSection({ text, placeholder, onChange }: Readonly<Props>) {
  return (
    <div className="space-y-2">
      <h2 className="text-xl font-semibold">About</h2>
      <EditableTextarea onChange={onChange} placeholder={placeholder} testId="about">
        {text}
      </EditableTextarea>
    </div>
  );
}
