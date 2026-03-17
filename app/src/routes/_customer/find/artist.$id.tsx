import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/find/artist/$id")({
  component: () => <div>Artist Details</div>,
});
