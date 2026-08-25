import { create } from "zustand";
import { produce } from "immer";
import type { Concert } from "../types";

interface ConcertStore {
  draft: Concert | undefined;
  editMode: boolean;
  isDirty: boolean;
  beginEdit: (concert: Concert) => void;
  endEdit: () => void;
  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setPrice: (price: number) => void;
  setTotalTickets: (totalTickets: number) => void;
}

const notEditing = {
  draft: undefined,
  editMode: false,
  isDirty: false,
};

export const useConcertStore = create<ConcertStore>((set) => ({
  ...notEditing,
  beginEdit: (concert) =>
    set({ ...notEditing, draft: { ...concert }, editMode: true }),
  endEdit: () => set(notEditing),
  setName: (name) =>
    set(
      produce((state: ConcertStore) => {
        if (!state.draft) return;
        state.draft.name = name;
        state.isDirty = true;
      }),
    ),
  setAbout: (about) =>
    set(
      produce((state: ConcertStore) => {
        if (!state.draft) return;
        state.draft.about = about;
        state.isDirty = true;
      }),
    ),
  setPrice: (price) =>
    set(
      produce((state: ConcertStore) => {
        if (!state.draft) return;
        state.draft.price = price;
        state.isDirty = true;
      }),
    ),
  setTotalTickets: (totalTickets) =>
    set(
      produce((state: ConcertStore) => {
        if (!state.draft) return;
        state.draft.totalTickets = totalTickets;
        state.isDirty = true;
      }),
    ),
}));
