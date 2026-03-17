import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/find/artist/$id")({
  component: () => <div>Artist Details</div>,
});
