import { createFileRoute } from "@tanstack/react-router";
import FailPage from "@/pages/FailPage";

export const Route = createFileRoute("/fail")({
  component: FailPage,
});
