import { create } from "zustand";
import type { Artist } from "@/types/artist";

interface ArtistStore {
  draft: Artist | null;
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
  draft: null,
  editMode: false,
  isDirty: false,
  image: undefined,

  toggleEdit: (artist) => set((state) => ({
    editMode: !state.editMode,
    draft: !state.editMode ? { ...artist } : null,
    isDirty: false,
    image: undefined,
  })),

  resetDraft: (artist) => set({
    draft: { ...artist },
    isDirty: false,
    image: undefined,
  }),

  setName: (name) => set((state) => ({
    draft: state.draft ? { ...state.draft, name } : null,
    isDirty: true,
  })),

  setAbout: (about) => set((state) => ({
    draft: state.draft ? { ...state.draft, about } : null,
    isDirty: true,
  })),

  setLocation: (latitude, longitude, county, town) => set((state) => ({
    draft: state.draft ? { ...state.draft, latitude, longitude, county, town } : null,
    isDirty: true,
  })),

  setImage: (file) => set((state) => ({
    draft: state.draft ? { ...state.draft, imageUrl: URL.createObjectURL(file) } : null,
    image: file,
    isDirty: true,
  })),
}));
