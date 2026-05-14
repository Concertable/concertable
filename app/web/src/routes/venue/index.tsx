import { createFileRoute } from "@tanstack/react-router";
import { VenueDashboardPage } from "@/features/venues";

export const Route = createFileRoute("/venue/")({
  component: VenueDashboardPage,
});
