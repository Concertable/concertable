import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/artist/my/concerts/concert/$id")({
  component: () => <div>My Concert</div>,
});
