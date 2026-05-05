import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import { defaultContract } from "@/features/contracts";
import type { Opportunity, OpportunityDraft } from "../types";
import type { Contract, PaymentMethod } from "@/features/contracts";
import type { Genre } from "@/types/common";

type StoredOpportunity = Opportunity | OpportunityDraft;

interface OpportunitiesStore {
  opportunities: StoredOpportunity[];
  isDirty: boolean;

  hydrate: (opportunities: Opportunity[]) => void;
  reset: () => void;
  add: (draft: OpportunityDraft) => void;
  remove: (index: number) => void;

  setDates: (index: number, start: string, end: string) => void;
  setContractType: (index: number, type: Contract["$type"]) => void;
  setContract: (index: number, contract: Contract) => void;
  setPaymentMethod: (index: number, method: PaymentMethod) => void;
  toggleGenre: (index: number, genre: Genre) => void;
}

export const useOpportunitiesStore = create<OpportunitiesStore>()(
  immer((set) => ({
    opportunities: [],
    isDirty: false,

    hydrate: (opportunities) => set((s) => { s.opportunities = opportunities; s.isDirty = false; }),
    reset: () => set((s) => { s.opportunities = []; s.isDirty = false; }),
    add: (draft) => set((s) => { s.opportunities.push(draft); s.isDirty = true; }),
    remove: (index) => set((s) => { s.opportunities.splice(index, 1); s.isDirty = true; }),

    setDates: (index, start, end) => set((s) => {
      s.opportunities[index].startDate = start;
      s.opportunities[index].endDate = end;
      s.isDirty = true;
    }),
    setContractType: (index, type) => set((s) => {
      s.opportunities[index].contract = defaultContract(type, s.opportunities[index].contract.paymentMethod);
      s.isDirty = true;
    }),
    setContract: (index, contract) => set((s) => {
      s.opportunities[index].contract = contract;
      s.isDirty = true;
    }),
    setPaymentMethod: (index, method) => set((s) => {
      s.opportunities[index].contract.paymentMethod = method;
      s.isDirty = true;
    }),
    toggleGenre: (index, genre) => set((s) => {
      const genres = s.opportunities[index].genres;
      const i = genres.findIndex((g) => g.id === genre.id);
      if (i >= 0) genres.splice(i, 1);
      else genres.push(genre);
      s.isDirty = true;
    }),
  }))
);
