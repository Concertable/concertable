import { create } from "zustand";
import { produce } from "immer";
import type { Artist } from "../types";

interface ArtistStore {
  draft: Artist | undefined;
  editMode: boolean;
  isDirty: boolean;
  banner: File | undefined;
  avatar: File | undefined;

  toggleEdit: (artist: Artist) => void;
  resetDraft: (artist: Artist) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setBanner: (file: File) => void;
  setAvatar: (file: File) => void;
}

export const useArtistStore = create<ArtistStore>((set) => ({
  draft: undefined,
  editMode: false,
  isDirty: false,
  banner: undefined,
  avatar: undefined,

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
        state.banner = undefined;
        state.avatar = undefined;
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

  setBanner: (file) =>
    set(
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.bannerUrl = URL.createObjectURL(file);
        state.banner = file;
        state.isDirty = true;
      }),
    ),

  setAvatar: (file) =>
    set(
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.avatar = URL.createObjectURL(file);
        state.avatar = file;
        state.isDirty = true;
      }),
    ),
}));
