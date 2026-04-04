import { createFileRoute } from "@tanstack/react-router";
import CreatePreferencePage from "@/pages/customer/profile/preferences/CreatePreferencePage";

export const Route = createFileRoute("/_customer/settings/preferences/create")({
  component: CreatePreferencePage,
});
