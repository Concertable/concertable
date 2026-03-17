import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/preferences/create")({
  component: () => <div>Create Preference</div>,
});
