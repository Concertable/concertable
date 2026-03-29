import { useEditableContext } from "@/providers/EditableProvider";

interface Props {
    edit: React.ReactNode;
    view: React.ReactNode;
}

export function Editable({ edit, view }: Readonly<Props>) {
    const editMode = useEditableContext();
    return editMode ? edit : view;
}