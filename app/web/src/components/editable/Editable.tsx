import { useEditableContext } from "@concertable/shared/providers";

interface Props {
  view: React.ReactNode;
  edit: React.ReactNode;
}

export function Editable({ view, edit }: Readonly<Props>) {
  const editMode = useEditableContext();
  return editMode ? edit : view;
}
