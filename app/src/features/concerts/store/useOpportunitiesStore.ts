import { create } from "zustand";
import { produce } from "immer";
import type { Opportunity, OpportunityDraft } from "../types";

type StoredOpportunity = Opportunity | OpportunityDraft;

interface OpportunitiesStore {
  opportunities: StoredOpportunity[];
  isDirty: boolean;

  hydrate: (opportunities: Opportunity[]) => void;
  reset: () => void;

  add: (opportunity: OpportunityDraft) => void;
  update: (index: number, opportunity: StoredOpportunity) => void;
  remove: (index: number) => void;
}

export const useOpportunitiesStore = create<OpportunitiesStore>((set) => ({
  opportunities: [],
  isDirty: false,

  hydrate: (opportunities) =>
    set(
      produce((state: OpportunitiesStore) => {
        state.opportunities = opportunities;
        state.isDirty = false;
      }),
    ),

  reset: () =>
    set(
      produce((state: OpportunitiesStore) => {
        state.opportunities = [];
        state.isDirty = false;
      }),
    ),

  add: (opportunity) =>
    set(
      produce((state: OpportunitiesStore) => {
        state.opportunities.push(opportunity);
        state.isDirty = true;
      }),
    ),

  update: (index, opportunity) =>
    set(
      produce((state: OpportunitiesStore) => {
        state.opportunities[index] = opportunity;
        state.isDirty = true;
      }),
    ),

  remove: (index) =>
    set(
      produce((state: OpportunitiesStore) => {
        state.opportunities.splice(index, 1);
        state.isDirty = true;
      }),
    ),
}));
