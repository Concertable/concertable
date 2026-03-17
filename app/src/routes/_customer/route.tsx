import { createFileRoute } from "@tanstack/react-router";
import { requireAuth } from "@/lib/guards";
import CustomerLayout from "@/layouts/CustomerLayout";

export const Route = createFileRoute("/_customer")({
  beforeLoad: () => requireAuth(),
  component: CustomerLayout,
});
