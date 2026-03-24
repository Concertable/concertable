import { createFileRoute } from "@tanstack/react-router";
import MyProfilePage from "@/pages/customer/profile/MyProfilePage";

export const Route = createFileRoute("/_customer/profile/my")({
  component: MyProfilePage,
});
