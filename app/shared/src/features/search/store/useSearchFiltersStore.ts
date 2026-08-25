import { create } from "zustand";
import type { SearchFilters } from "../schemas/searchSchema";

interface SearchFiltersState {
  filters: SearchFilters;
  setFilters: (filters: SearchFilters) => void;
  replaceFilters: (filters: SearchFilters) => void;
  updateFilters: (filters: Partial<SearchFilters>) => void;
}

export const useSearchFiltersStore = create<SearchFiltersState>((set) => ({
  filters: { headerType: "concert" },
  setFilters: (filters) => set({ filters }),
  replaceFilters: (filters) => set({ filters }),
  updateFilters: (filters) =>
    set((state) => ({ filters: { ...state.filters, ...filters } })),
}));
