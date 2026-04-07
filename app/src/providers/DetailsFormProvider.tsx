import { createContext, useContext } from "react";

interface DetailsFormContextValue {
  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setBanner: (file: File) => void;
  setAvatar: (file: File) => void;
}

const DetailsFormContext = createContext<DetailsFormContextValue | null>(null);

export function DetailsFormProvider({
  children,
  setName,
  setAbout,
  setBanner,
  setAvatar,
}: DetailsFormContextValue & { children: React.ReactNode }) {
  return (
    <DetailsFormContext.Provider
      value={{ setName, setAbout, setBanner, setAvatar }}
    >
      {children}
    </DetailsFormContext.Provider>
  );
}

export function useDetailsForm() {
  const ctx = useContext(DetailsFormContext);
  if (!ctx)
    throw new Error("useDetailsForm must be used within a DetailsFormProvider");
  return ctx;
}
