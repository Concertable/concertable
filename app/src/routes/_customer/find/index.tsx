import { createFileRoute } from "@tanstack/react-router";
import FindPage from "@/pages/customer/FindPage";
import type { SearchFilters } from "@/components/SearchBar";
import type { HeaderType } from "@/types/header";

export const Route = createFileRoute("/_customer/find/")({
  component: FindPage,
  validateSearch: (search): SearchFilters => ({
    query: (search.query as string) ?? undefined,
    headerType: (search.headerType as HeaderType) ?? "concert",
    location: search.location as SearchFilters["location"],
    from: (search.from as string) ?? undefined,
    to: (search.to as string) ?? undefined,
  }),
});
