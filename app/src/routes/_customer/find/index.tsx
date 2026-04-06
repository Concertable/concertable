import { createFileRoute } from "@tanstack/react-router";
import FindPage from "@/pages/customer/FindPage";
import { SearchSchema } from "@/schemas/searchSchema";

export const Route = createFileRoute("/_customer/find/")({
  component: FindPage,
  validateSearch: SearchSchema(),
});
