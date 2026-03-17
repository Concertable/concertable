import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/preferences/")({
  component: () => <div>My Preferences</div>,
});
