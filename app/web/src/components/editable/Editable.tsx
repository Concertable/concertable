import { useEditableContext } from "@/providers/EditableProvider";

interface Props {
  view: React.ReactNode;
  edit: React.ReactNode;
}

export function Editable({ view, edit }: Readonly<Props>) {
  const editMode = useEditableContext();
  return editMode ? edit : view;
}
