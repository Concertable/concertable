import { create } from "zustand";
import { produce } from "immer";
import type { Venue } from "../types";
import type { ImageFile } from "../../../types/image";

interface VenueStore {
  draft: Venue | undefined;
  editMode: boolean;
  isDirty: boolean;
  banner: ImageFile | undefined;
  avatar: ImageFile | undefined;

  beginEdit: (venue: Venue) => void;
  endEdit: () => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setBanner: (file: ImageFile) => void;
  setAvatar: (file: ImageFile) => void;
}

const notEditing = {
  draft: undefined,
  editMode: false,
  isDirty: false,
  banner: undefined,
  avatar: undefined,
};

export const useVenueStore = create<VenueStore>((set) => ({
  ...notEditing,

  beginEdit: (venue) => set({ ...notEditing, draft: { ...venue }, editMode: true }),

  endEdit: () => set(notEditing),

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
        state.draft.bannerUrl = file.uri;
        state.banner = file;
        state.isDirty = true;
      }),
    ),

  setAvatar: (file) =>
    set(
      produce((state: VenueStore) => {
        if (!state.draft) return;
        state.draft.avatar = file.uri;
        state.avatar = file;
        state.isDirty = true;
      }),
    ),
}));
