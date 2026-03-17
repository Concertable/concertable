import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/artist/")({
  component: () => <div>Artist Dashboard</div>,
});
