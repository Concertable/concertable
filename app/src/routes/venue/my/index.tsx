import { createFileRoute } from "@tanstack/react-router";
import MyVenuePage from "@/pages/venue/my/MyVenuePage";

export const Route = createFileRoute("/venue/my/")({
  component: MyVenuePage,
});
