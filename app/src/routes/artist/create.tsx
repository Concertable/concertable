import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/artist/create")({
  component: () => <div>Create Artist</div>,
});
