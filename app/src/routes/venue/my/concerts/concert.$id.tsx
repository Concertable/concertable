import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/my/concerts/concert/$id")({
  component: () => <div>My Concert</div>,
});
