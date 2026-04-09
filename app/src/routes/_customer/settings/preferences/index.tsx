import { createFileRoute } from "@tanstack/react-router";
import PreferencesPage from "@/pages/customer/profile/preferences/PreferencesPage";
import { requireRole } from "@/lib/guards";

export const Route = createFileRoute("/_customer/settings/preferences/")({
  beforeLoad: () => requireRole("Customer"),
  component: PreferencesPage,
});
