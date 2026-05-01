import { create } from "zustand";
import { produce } from "immer";
import type { Venue } from "../types";

interface VenueStore {
  draft: Venue | undefined;
  editMode: boolean;
  isDirty: boolean;
  banner: File | undefined;
  avatar: File | undefined;

  toggleEdit: (venue: Venue) => void;
  resetDraft: (venue: Venue) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setBanner: (file: File) => void;
  setAvatar: (file: File) => void;
}

export const useVenueStore = create<VenueStore>((set) => ({
  draft: undefined,
  editMode: false,
  isDirty: false,
  banner: undefined,
  avatar: undefined,

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
        state.banner = undefined;
        state.avatar = undefined;
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

  setBanner: (file) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.bannerUrl = URL.createObjectURL(file);
        state.banner = file;
        state.isDirty = true;
      }),
    ),

  setAvatar: (file) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.avatar = URL.createObjectURL(file);
        state.avatar = file;
        state.isDirty = true;
      }),
    ),
}));
