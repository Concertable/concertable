import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/")({
  component: () => <div>Profile Menu</div>,
});
