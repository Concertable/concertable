import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import type { ImageFile } from "../../../types/image";
import type { Artist } from "../types";

export interface ArtistState {
  draft: Artist | undefined;
  banner: ImageFile | undefined;
  avatar: ImageFile | undefined;
  editMode: boolean;
  isDirty: boolean;
  beginEdit: (artist: Artist) => void;
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

export const useArtistStore = create<ArtistState>()(
  immer((set) => ({
    draft: undefined,
    banner: undefined,
    avatar: undefined,
    editMode: false,
    isDirty: false,
    beginEdit: (artist) =>
      set((state) => {
        state.draft = {
          id: artist.id,
          name: artist.name,
          about: artist.about,
          bannerUrl: artist.bannerUrl,
          avatar: artist.avatar,
          rating: artist.rating,
          genres: [...artist.genres],
          email: artist.email,
          county: artist.county,
          town: artist.town,
          latitude: artist.latitude,
          longitude: artist.longitude,
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
