import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/fail")({
  component: () => <div>Fail</div>,
});
