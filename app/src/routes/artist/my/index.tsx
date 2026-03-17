import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/artist/my/")({
  component: () => <div>My Artist</div>,
});
