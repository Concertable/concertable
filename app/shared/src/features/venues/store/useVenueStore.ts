import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import type { ImageFile } from "../../../types/image";
import type { Venue } from "../types";

export interface VenueState {
  draft: Venue | undefined;
  banner: ImageFile | undefined;
  avatar: ImageFile | undefined;
  editMode: boolean;
  isDirty: boolean;
  beginEdit: (venue: Venue) => void;
  endEdit: () => void;
  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setBanner: (banner: ImageFile) => void;
  setAvatar: (avatar: ImageFile) => void;
  setLocation: (
    latitude: number,
    longitude: number,
    county: string,
    town: string,
  ) => void;
}

export const useVenueStore = create<VenueState>()(
  immer((set) => ({
    draft: undefined,
    banner: undefined,
    avatar: undefined,
    editMode: false,
    isDirty: false,
    beginEdit: (venue) =>
      set((state) => {
        state.draft = {
          id: venue.id,
          name: venue.name,
          about: venue.about,
          bannerUrl: venue.bannerUrl,
          avatar: venue.avatar,
          rating: venue.rating,
          email: venue.email,
          county: venue.county,
          town: venue.town,
          latitude: venue.latitude,
          longitude: venue.longitude,
        };
        state.banner = undefined;
        state.avatar = undefined;
        state.editMode = true;
        state.isDirty = false;
      }),
    endEdit: () =>
      set((state) => {
        state.draft = undefined;
        state.banner = undefined;
        state.avatar = undefined;
        state.editMode = false;
        state.isDirty = false;
      }),
    setName: (name) =>
      set((state) => {
        if (!state.draft) return;
        state.draft.name = name;
        state.isDirty = true;
      }),
    setAbout: (about) =>
      set((state) => {
        if (!state.draft) return;
        state.draft.about = about;
        state.isDirty = true;
      }),
    setBanner: (banner) =>
      set((state) => {
        if (!state.draft) return;
        state.draft.bannerUrl = banner.uri;
        state.banner = banner;
        state.isDirty = true;
      }),
    setAvatar: (avatar) =>
      set((state) => {
        if (!state.draft) return;
        state.draft.avatar = avatar.uri;
        state.avatar = avatar;
        state.isDirty = true;
      }),
    setLocation: (latitude, longitude, county, town) =>
      set((state) => {
        if (!state.draft) return;
        state.draft.latitude = latitude;
        state.draft.longitude = longitude;
        state.draft.county = county;
        state.draft.town = town;
        state.isDirty = true;
      }),
  })),
);
