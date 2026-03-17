import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/artist/find/venue/$id")({
  component: () => <div>Venue Details</div>,
});
