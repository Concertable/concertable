import { createFileRoute } from "@tanstack/react-router";
import FindPage from "@/pages/customer/FindPage";

export const Route = createFileRoute("/_customer/find/")({
  component: FindPage,
});
