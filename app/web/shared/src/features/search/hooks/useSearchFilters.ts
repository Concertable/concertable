import { useNavigate, useRouterState, useSearch } from "@tanstack/react-router";
import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";
import { useSearchFilters as useSharedSearchFilters } from "@concertable/shared/features/search";
import type { SearchFilters } from "../schemas/searchSchema";

export function useSearchFilters() {
  const { filters, replaceFilters, updateFilters } = useSharedSearchFilters();
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const navigate = useNavigate();
  const routeFilters = useSearch({ strict: false }) as SearchFilters;

  useMountEffect(() => replaceFilters(routeFilters));

  function applyFilters() {
    navigate({ to: pathname, search: () => filters });
  }

  return { filters, replaceFilters, updateFilters, applyFilters };
}
