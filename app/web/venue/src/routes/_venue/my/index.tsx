import { createFileRoute } from "@tanstack/react-router";
import { MyVenuePage } from "@/features/venues";

export const Route = createFileRoute("/_venue/my/")({
  component: MyVenuePage,
});
