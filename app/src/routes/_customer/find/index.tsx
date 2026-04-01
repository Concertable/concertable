import { createFileRoute } from "@tanstack/react-router";
import FindPage from "@/pages/customer/FindPage";
import { searchSchema } from "@/lib/searchParams";

export const Route = createFileRoute("/_customer/find/")({
  component: FindPage,
  validateSearch: searchSchema("concert"),
});
