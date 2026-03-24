import { createFileRoute } from "@tanstack/react-router";
import CreateVenuePage from "@/pages/venue/CreateVenuePage";

export const Route = createFileRoute("/venue/create")({
  component: CreateVenuePage,
});
