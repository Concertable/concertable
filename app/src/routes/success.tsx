import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/success")({
  component: () => <div>Success</div>,
});
