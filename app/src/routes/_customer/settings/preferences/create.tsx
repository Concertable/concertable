import { createFileRoute } from "@tanstack/react-router";
import CreatePreferencePage from "@/pages/customer/profile/preferences/CreatePreferencePage";
import { requireRole } from "@/lib/guards";

export const Route = createFileRoute("/_customer/settings/preferences/create")({
  beforeLoad: () => requireRole("Customer"),
  component: CreatePreferencePage,
});
