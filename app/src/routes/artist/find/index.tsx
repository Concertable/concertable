import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/artist/find/")({
  component: () => <div>Find Venues</div>,
});
