import { createFileRoute } from "@tanstack/react-router";
import CustomerLayout from "@/layouts/CustomerLayout";

export const Route = createFileRoute("/_customer")({
  component: CustomerLayout,
});
