import { createFileRoute } from "@tanstack/react-router";
import PreferencesPage from "@/pages/customer/profile/preferences/PreferencesPage";

export const Route = createFileRoute("/_customer/settings/preferences/")({
  component: PreferencesPage,
});
