import { createFileRoute } from "@tanstack/react-router";
import HomePage from "@/pages/customer/HomePage";

export const Route = createFileRoute("/_customer/")({
  component: HomePage,
});
