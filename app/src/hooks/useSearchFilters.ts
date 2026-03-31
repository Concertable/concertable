import { useEffect } from "react";
import { useNavigate, useSearch } from "@tanstack/react-router";
import { useSearchFiltersStore } from "@/store/useSearchFiltersStore";
import type { SearchFilters } from "@/components/SearchBar";
import type { HeaderType } from "@/types/header";

export function useSearchFilters(defaultHeaderType: HeaderType) {
  const { filters, setFilters } = useSearchFiltersStore();
  const navigate = useNavigate();
  const search = useSearch({ strict: false }) as Partial<SearchFilters>;

  useEffect(() => {
    setFilters({ headerType: defaultHeaderType, ...search });
  }, []);

  function updateFilters(next: SearchFilters) {
    setFilters(next);
    navigate({ to: ".", search: next });
  }

  return { filters, updateFilters };
}
