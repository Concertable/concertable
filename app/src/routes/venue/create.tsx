import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/create")({
  component: () => <div>Create Venue</div>,
});
