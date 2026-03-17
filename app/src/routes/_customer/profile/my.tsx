import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/profile/my")({
  component: () => <div>My Profile</div>,
});
