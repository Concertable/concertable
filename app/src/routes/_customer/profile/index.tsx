import { createFileRoute } from "@tanstack/react-router";
import ProfilePage from "@/pages/customer/profile/ProfilePage";

export const Route = createFileRoute("/_customer/profile/")({
  component: ProfilePage,
});
