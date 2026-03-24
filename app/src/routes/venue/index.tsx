import { createFileRoute } from "@tanstack/react-router";
import VenueDashboardPage from "@/pages/venue/VenueDashboardPage";

export const Route = createFileRoute("/venue/")({
  component: VenueDashboardPage,
});
