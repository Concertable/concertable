import { create } from "zustand";
import { produce } from "immer";
import type { Venue } from "@/types/venue";

interface VenueStore {
  draft: Venue | undefined;
  editMode: boolean;
  isDirty: boolean;
  image: File | undefined;

  toggleEdit: (venue: Venue) => void;
  resetDraft: (venue: Venue) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setImage: (file: File) => void;
}

export const useVenueStore = create<VenueStore>((set) => ({
  draft: undefined,
  editMode: false,
  isDirty: false,
  image: undefined,

  toggleEdit: (venue) =>
    set(
      produce((state: VenueStore) => {
        state.editMode = !state.editMode;
        if (state.editMode) {
          state.draft = state.draft ?? { ...venue };
        }
      }),
    ),

  resetDraft: (venue) =>
    set(
      produce((state: VenueStore) => {
        state.draft = { ...venue };
        state.editMode = false;
        state.isDirty = false;
        state.image = undefined;
      }),
    ),

  setName: (name) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.name = name;
        state.isDirty = true;
      }),
    ),

  setAbout: (about) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.about = about;
        state.isDirty = true;
      }),
    ),

  setLocation: (latitude, longitude, county, town) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.latitude = latitude;
        state.draft.longitude = longitude;
        state.draft.county = county;
        state.draft.town = town;
        state.isDirty = true;
      }),
    ),

  setImage: (file) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.imageUrl = URL.createObjectURL(file);
        state.image = file;
        state.isDirty = true;
      }),
    ),
}));
