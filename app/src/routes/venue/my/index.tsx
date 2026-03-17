import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/my/")({
  component: () => <div>My Venue</div>,
});
