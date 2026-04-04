import { createContext, useContext } from "react";

const EditableContext = createContext(false);

export function EditableProvider({
  editMode,
  children,
}: {
  editMode: boolean;
  children: React.ReactNode;
}) {
  return (
    <EditableContext.Provider value={editMode}>
      {children}
    </EditableContext.Provider>
  );
}

export function useEditableContext() {
  return useContext(EditableContext);
}
