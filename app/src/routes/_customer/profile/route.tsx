import { createFileRoute } from "@tanstack/react-router";
import ProfileLayout from "@/layouts/ProfileLayout";

export const Route = createFileRoute("/_customer/profile")({
  component: ProfileLayout,
});
