import { create } from "zustand";
import type { SearchFilters } from "../schemas/searchSchema";

interface SearchFiltersState {
  filters: SearchFilters;
  replaceFilters: (filters: SearchFilters) => void;
  updateFilters: (filters: Partial<SearchFilters>) => void;
}

export const useSearchFiltersStore = create<SearchFiltersState>((set) => ({
  filters: { headerType: "concert" },
  replaceFilters: (filters) => set({ filters }),
  updateFilters: (filters) =>
    set((state) => ({ filters: { ...state.filters, ...filters } })),
}));
