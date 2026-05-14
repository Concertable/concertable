import { create } from "zustand";
import type { SearchFilters } from "../schemas/searchSchema";

interface SearchFiltersState {
  filters: SearchFilters;
  setFilters: (filters: SearchFilters) => void;
}

export const useSearchFiltersStore = create<SearchFiltersState>((set) => ({
  filters: { headerType: "concert" },
  setFilters: (filters) => set({ filters }),
}));
