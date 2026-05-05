import { create } from "zustand";
import { immer } from "zustand/middleware/immer";
import { defaultContract } from "@/features/contracts";
import type { Opportunity, OpportunityDraft } from "../types";
import type { Contract, PaymentMethod } from "@/features/contracts";
import type { Genre } from "@/types/common";

interface OpportunitiesStore {
  opportunities: Opportunity[];
  drafts: OpportunityDraft[];
  isDirty: boolean;

  hydrate: (opportunities: Opportunity[]) => void;
  reset: () => void;
  addDraft: (draft: OpportunityDraft) => void;

  removeOpportunity: (index: number) => void;
  removeDraft: (index: number) => void;

  setOpportunityDates: (index: number, start: string, end: string) => void;
  setOpportunityContractType: (index: number, type: Contract["$type"]) => void;
  setOpportunityContract: (index: number, contract: Contract) => void;
  setOpportunityPaymentMethod: (index: number, method: PaymentMethod) => void;
  toggleOpportunityGenre: (index: number, genre: Genre) => void;

  setDraftDates: (index: number, start: string, end: string) => void;
  setDraftContractType: (index: number, type: Contract["$type"]) => void;
  setDraftContract: (index: number, contract: Contract) => void;
  setDraftPaymentMethod: (index: number, method: PaymentMethod) => void;
  toggleDraftGenre: (index: number, genre: Genre) => void;
}

export const useOpportunitiesStore = create<OpportunitiesStore>()(
  immer((set) => ({
    opportunities: [],
    drafts: [],
    isDirty: false,

    hydrate: (opportunities) => set((s) => { s.opportunities = opportunities; s.drafts = []; s.isDirty = false; }),
    reset: () => set((s) => { s.opportunities = []; s.drafts = []; s.isDirty = false; }),
    addDraft: (draft) => set((s) => { s.drafts.push(draft); s.isDirty = true; }),

    removeOpportunity: (index) => set((s) => { s.opportunities.splice(index, 1); s.isDirty = true; }),
    removeDraft: (index) => set((s) => { s.drafts.splice(index, 1); s.isDirty = true; }),

    setOpportunityDates: (index, start, end) => set((s) => { s.opportunities[index].startDate = start; s.opportunities[index].endDate = end; s.isDirty = true; }),
    setOpportunityContractType: (index, type) => set((s) => { s.opportunities[index].contract = defaultContract(type, s.opportunities[index].contract.paymentMethod); s.isDirty = true; }),
    setOpportunityContract: (index, contract) => set((s) => { s.opportunities[index].contract = contract; s.isDirty = true; }),
    setOpportunityPaymentMethod: (index, method) => set((s) => { s.opportunities[index].contract.paymentMethod = method; s.isDirty = true; }),
    toggleOpportunityGenre: (index, genre) => set((s) => {
      const genres = s.opportunities[index].genres;
      const i = genres.findIndex((g) => g.id === genre.id);
      if (i >= 0) genres.splice(i, 1); else genres.push(genre);
      s.isDirty = true;
    }),

    setDraftDates: (index, start, end) => set((s) => { s.drafts[index].startDate = start; s.drafts[index].endDate = end; s.isDirty = true; }),
    setDraftContractType: (index, type) => set((s) => { s.drafts[index].contract = defaultContract(type, s.drafts[index].contract.paymentMethod); s.isDirty = true; }),
    setDraftContract: (index, contract) => set((s) => { s.drafts[index].contract = contract; s.isDirty = true; }),
    setDraftPaymentMethod: (index, method) => set((s) => { s.drafts[index].contract.paymentMethod = method; s.isDirty = true; }),
    toggleDraftGenre: (index, genre) => set((s) => {
      const genres = s.drafts[index].genres;
      const i = genres.findIndex((g) => g.id === genre.id);
      if (i >= 0) genres.splice(i, 1); else genres.push(genre);
      s.isDirty = true;
    }),
  }))
);
