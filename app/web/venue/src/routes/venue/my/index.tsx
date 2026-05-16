import { createFileRoute } from "@tanstack/react-router";
import { MyVenuePage } from "@/features/venues";

export const Route = createFileRoute("/venue/my/")({
  component: MyVenuePage,
});
