import { create } from "zustand";
import { produce } from "immer";
import type { Artist } from "@/types/artist";

interface ArtistStore {
  draft: Artist | undefined;
  editMode: boolean;
  isDirty: boolean;
  image: File | undefined;

  toggleEdit: (artist: Artist) => void;
  resetDraft: (artist: Artist) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setImage: (file: File) => void;
}

export const useArtistStore = create<ArtistStore>((set) => ({
  draft: undefined,
  editMode: false,
  isDirty: false,
  image: undefined,

  toggleEdit: (artist) =>
    set(
      produce((state: ArtistStore) => {
        state.editMode = !state.editMode;
        if (state.editMode) {
          state.draft = state.draft ?? { ...artist };
        }
      }),
    ),

  resetDraft: (artist) =>
    set(
      produce((state: ArtistStore) => {
        state.draft = { ...artist };
        state.editMode = false;
        state.isDirty = false;
        state.image = undefined;
      }),
    ),

  setName: (name) =>
    set(
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.name = name;
        state.isDirty = true;
      }),
    ),

  setAbout: (about) =>
    set(
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.about = about;
        state.isDirty = true;
      }),
    ),

  setLocation: (latitude, longitude, county, town) =>
    set(
      produce((state: ArtistStore) => {
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
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.imageUrl = URL.createObjectURL(file);
        state.image = file;
        state.isDirty = true;
      }),
    ),
}));
