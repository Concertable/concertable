import { createContext, useContext } from "react";
import type { ImageFile } from "@concertable/shared";

interface DetailsFormContextValue {
  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setBanner: (file: ImageFile) => void;
  setAvatar: (file: ImageFile) => void;
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
