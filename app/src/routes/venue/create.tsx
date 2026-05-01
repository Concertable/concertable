import { createFileRoute } from "@tanstack/react-router";
import { CreateVenuePage } from "@/features/venues";

export const Route = createFileRoute("/venue/create")({
  component: CreateVenuePage,
});
