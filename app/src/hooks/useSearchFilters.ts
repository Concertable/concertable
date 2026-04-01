import { useNavigate, useSearch } from "@tanstack/react-router";
import { useSearchFiltersStore } from "@/store/useSearchFiltersStore";
import { useMountEffect } from "@/hooks/useMountEffect";
import type { SearchFilters } from "@/components/SearchBar";

export function useSearchFilters() {
  const { setFilters } = useSearchFiltersStore();
  const navigate = useNavigate();
  const filters = useSearch({ strict: false }) as SearchFilters;

  useMountEffect(() => setFilters(filters));

  function updateFilters(next: SearchFilters) {
    setFilters(next);
    navigate({ search: next });
  }

  return { filters, updateFilters };
}
