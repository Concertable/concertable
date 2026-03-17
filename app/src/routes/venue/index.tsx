import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/")({
  component: () => <div>Venue Dashboard</div>,
});
