import { create } from "zustand";
import { produce } from "immer";
import type { Venue } from "../types";
import type { OpportunityRequest } from "@/features/concerts/types";

interface VenueStore {
  draft: Venue | undefined;
  editMode: boolean;
  isDirty: boolean;
  banner: File | undefined;
  avatar: File | undefined;
  opportunities: OpportunityRequest[];

  toggleEdit: (venue: Venue) => void;
  resetDraft: (venue: Venue) => void;

  setName: (name: string) => void;
  setAbout: (about: string) => void;
  setLocation: (lat: number, lng: number, county: string, town: string) => void;
  setBanner: (file: File) => void;
  setAvatar: (file: File) => void;

  setOpportunities: (opportunities: OpportunityRequest[]) => void;
  addOpportunity: (opportunity: OpportunityRequest) => void;
  updateOpportunity: (index: number, opportunity: OpportunityRequest) => void;
  removeOpportunity: (index: number) => void;
}

export const useVenueStore = create<VenueStore>((set) => ({
  draft: undefined,
  editMode: false,
  isDirty: false,
  banner: undefined,
  avatar: undefined,
  opportunities: [],

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
        state.opportunities = [];
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

  setOpportunities: (opportunities) =>
    set(
      produce((state: VenueStore) => {
        state.opportunities = opportunities;
      }),
    ),

  addOpportunity: (opportunity) =>
    set(
      produce((state: VenueStore) => {
        state.opportunities.push(opportunity);
        state.isDirty = true;
      }),
    ),

  updateOpportunity: (index, opportunity) =>
    set(
      produce((state: VenueStore) => {
        state.opportunities[index] = opportunity;
        state.isDirty = true;
      }),
    ),

  removeOpportunity: (index) =>
    set(
      produce((state: VenueStore) => {
        state.opportunities.splice(index, 1);
        state.isDirty = true;
      }),
    ),
}));
