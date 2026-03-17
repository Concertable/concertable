import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_customer/find/concert/$id")({
  component: () => <div>Concert Details</div>,
});
