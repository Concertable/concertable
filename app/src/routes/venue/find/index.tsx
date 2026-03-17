import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/find/")({
  component: () => <div>Find Artists</div>,
});
