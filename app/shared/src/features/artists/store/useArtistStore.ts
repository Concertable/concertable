import { create } from "zustand";
import { produce } from "immer";
import type { Artist } from "../types";
import type { ImageFile } from "../../../types/image";

interface ArtistStore {
  draft: Artist | undefined;
  editMode: boolean;
  isDirty: boolean;
  banner: ImageFile | undefined;
  avatar: ImageFile | undefined;

  toggleEdit: (artist: Artist) => void;
  resetDraft: (artist: Artist) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setBanner: (file: ImageFile) => void;
  setAvatar: (file: ImageFile) => void;
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
        (state.draft as any).latitude = latitude;
        (state.draft as any).longitude = longitude;
        state.draft.county = county;
        state.draft.town = town;
        state.isDirty = true;
      }),
    ),

  setBanner: (file) =>
    set(
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.bannerUrl = file.uri;
        state.banner = file;
        state.isDirty = true;
      }),
    ),

  setAvatar: (file) =>
    set(
      produce((state: ArtistStore) => {
        if (!state.draft) return;
        state.draft.avatar = file.uri;
        state.avatar = file;
        state.isDirty = true;
      }),
    ),
}));
