import { createFileRoute } from "@tanstack/react-router";
import ApplicationsPage from "@/pages/venue/my/ApplicationsPage";

export const Route = createFileRoute("/venue/my/applications/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: ApplicationsPage,
});
