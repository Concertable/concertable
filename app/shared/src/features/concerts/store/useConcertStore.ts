import { create } from "zustand";
import { produce } from "immer";
import type { Concert } from "../types";

interface ConcertStore {
  draft: Concert | undefined;
  editMode: boolean;
  isDirty: boolean;

  toggleEdit: (concert: Concert) => void;
  resetDraft: (concert: Concert) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setPrice: (price: number) => void;
  setTotalTickets: (totalTickets: number) => void;
}

export const useConcertStore = create<ConcertStore>((set) => ({
  draft: undefined,
  editMode: false,
  isDirty: false,

  toggleEdit: (concert) =>
    set(
      produce((state: ConcertStore) => {
        state.editMode = !state.editMode;
        if (state.editMode) {
          state.draft = state.draft ?? { ...concert };
        }
      }),
    ),

  resetDraft: (concert) =>
    set(
      produce((state: ConcertStore) => {
        state.draft = { ...concert };
        state.editMode = false;
        state.isDirty = false;
      }),
    ),

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
