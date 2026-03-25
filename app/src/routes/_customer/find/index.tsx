import { createFileRoute } from "@tanstack/react-router";
import FindPage from "@/pages/customer/FindPage";
import { validateSearchFilters } from "@/lib/searchParams";

export const Route = createFileRoute("/_customer/find/")({
  component: FindPage,
  validateSearch: validateSearchFilters("concert"),
});
