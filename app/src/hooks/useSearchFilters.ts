import { useNavigate } from "@tanstack/react-router";
import { Route } from "@/routes/_customer/find/index";
import type { SearchFilters } from "@/components/SearchBar";

export function useSearchFilters() {
  const filters = Route.useSearch();
  const navigate = useNavigate();

  function setFilters(filters: SearchFilters) {
    navigate({ to: "/find", search: filters });
  }

  return { filters, setFilters };
}
