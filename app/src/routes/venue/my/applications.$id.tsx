import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/venue/my/applications/$id")({
  component: () => <div>Listing Applications</div>,
});
