import { useSearchFiltersStore } from "../store/useSearchFiltersStore";

export function useSearchFilters() {
  const filters = useSearchFiltersStore((state) => state.filters);
  const replaceFilters = useSearchFiltersStore(
    (state) => state.replaceFilters,
  );
  const updateFilters = useSearchFiltersStore((state) => state.updateFilters);

  return { filters, replaceFilters, updateFilters };
}
