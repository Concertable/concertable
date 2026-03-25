import { create } from "zustand";
import type { Venue } from "@/types/venue";

interface VenueStore {
  draft: Venue | null;
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
  draft: null,
  editMode: false,
  isDirty: false,
  image: undefined,

  toggleEdit: (venue) => set((state) => ({
    editMode: !state.editMode,
    draft: !state.editMode ? { ...venue } : null,
    isDirty: false,
    image: undefined,
  })),

  resetDraft: (venue) => set({
    draft: { ...venue },
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
